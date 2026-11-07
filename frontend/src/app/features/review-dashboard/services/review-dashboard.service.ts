import { Injectable, signal, computed, DestroyRef, inject } from '@angular/core';
import { StreamNodeEvent, FinalReviewReport, NodeStepState } from '../models/review.models';

@Injectable({
  providedIn: 'root'
})
export class ReviewDashboardService {
  private readonly destroyRef = inject(DestroyRef);
  private eventSource: EventSource | null = null;

  // Primary Reactive Signals
  readonly isRunning = signal<boolean>(false);
  readonly error = signal<string | null>(null);
  readonly detectedLanguage = signal<string | null>(null);
  readonly reviewReport = signal<FinalReviewReport | null>(null);

  // Stepper Node Progression State
  readonly steps = signal<NodeStepState[]>([
    { name: 'supervisor', displayName: 'Supervisor (Routing)', status: 'pending', findingsCount: 0 },
    { name: 'architecture_node', displayName: 'Architecture Specialist', status: 'pending', findingsCount: 0 },
    { name: 'security_node', displayName: 'Security Specialist', status: 'pending', findingsCount: 0 },
    { name: 'performance_node', displayName: 'Performance Specialist', status: 'pending', findingsCount: 0 },
    { name: 'synthesis_node', displayName: 'Synthesis & Scoring', status: 'pending', findingsCount: 0 }
  ]);

  // Computed Selectors
  readonly overallScore = computed(() => this.reviewReport()?.overall_score ?? 0);
  readonly domainScores = computed(() => this.reviewReport()?.domain_scores ?? []);
  readonly findings = computed(() => this.reviewReport()?.findings ?? []);

  constructor() {
    this.destroyRef.onDestroy(() => this.terminateStream());
  }

  public startReviewStream(documentId: string): void {
    this.terminateStream();
    this.resetState();
    this.isRunning.set(true);

    // Initial step state: supervisor is actively running
    this.steps.update(steps =>
      steps.map(step =>
        step.name === 'supervisor' ? { ...step, status: 'running' as const } : step
      )
    );

    const streamUrl = `/api/documents/${documentId}/review/stream`;
    this.eventSource = new EventSource(streamUrl);

    this.eventSource.onmessage = (event: MessageEvent) => {
      try {
        const payload: StreamNodeEvent = JSON.parse(event.data);
        this.processStreamEvent(payload);
      } catch (err) {
        console.error('Failed to parse SSE payload', err);
      }
    };

    this.eventSource.onerror = () => {
      // EventSource triggers error when stream cleanly closes on server side
      if (this.reviewReport() !== null) {
        this.terminateStream();
      } else {
        this.error.set('Connection lost during agent review streaming.');
        this.terminateStream();
      }
    };
  }

  private processStreamEvent(event: StreamNodeEvent): void {
    if (event.detected_language) {
      this.detectedLanguage.set(event.detected_language);
    }

    // Update individual stepper progression
    this.steps.update(steps => {
      const updated = steps.map(step => {
        if (step.name === event.node) {
          return {
            ...step,
            status: 'completed' as const,
            findingsCount: event.findings_delta_count
          };
        }
        return step;
      });

      // If supervisor completed, trigger specialists into running
      if (event.node === 'supervisor') {
        return updated.map(s =>
          ['architecture_node', 'security_node', 'performance_node'].includes(s.name) && s.status === 'pending'
            ? { ...s, status: 'running' as const }
            : s
        );
      }

      // If all three specialists completed, trigger synthesis into running
      const specialistsComplete = updated
        .filter(s => ['architecture_node', 'security_node', 'performance_node'].includes(s.name))
        .every(s => s.status === 'completed');

      if (specialistsComplete) {
        return updated.map(s =>
          s.name === 'synthesis_node' && s.status === 'pending'
            ? { ...s, status: 'running' as const }
            : s
        );
      }

      return updated;
    });

    // Terminal payload processing
    if (event.is_final && event.report) {
      this.reviewReport.set(event.report);
      this.steps.update(steps =>
        steps.map(s => (s.name === 'synthesis_node' ? { ...s, status: 'completed' as const } : s))
      );
      this.terminateStream();
    }
  }

  public terminateStream(): void {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
    }
    this.isRunning.set(false);
  }

  private resetState(): void {
    this.error.set(null);
    this.reviewReport.set(null);
    this.detectedLanguage.set(null);
    this.steps.set([
      { name: 'supervisor', displayName: 'Supervisor (Routing)', status: 'pending', findingsCount: 0 },
      { name: 'architecture_node', displayName: 'Architecture Specialist', status: 'pending', findingsCount: 0 },
      { name: 'security_node', displayName: 'Security Specialist', status: 'pending', findingsCount: 0 },
      { name: 'performance_node', displayName: 'Performance Specialist', status: 'pending', findingsCount: 0 },
      { name: 'synthesis_node', displayName: 'Synthesis & Scoring', status: 'pending', findingsCount: 0 }
    ]);
  }
}
