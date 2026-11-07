import { Component, input, output, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewDashboardService } from '../../services/review-dashboard.service';
import { CodeFinding, FindingSeverity, DomainType } from '../../models/review.models';

@Component({
  selector: 'app-review-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './review-dashboard.component.html',
  styleUrls: ['./review-dashboard.component.scss']
})
export class ReviewDashboardComponent {
  readonly reviewService = inject(ReviewDashboardService);

  // Inputs & Outputs to integrate with Document Editor
  readonly documentId = input<string | null | undefined>();
  readonly onApplyPatch = output<{ problematicCode: string; patchCode: string }>();
  readonly close = output<void>();

  // Filter Signals
  readonly selectedSeverity = signal<FindingSeverity | 'All'>('All');
  readonly selectedDomain = signal<DomainType | 'All'>('All');

  // Track findings that have had their patch applied in the current session
  readonly appliedPatchIds = signal<Set<string>>(new Set());

  // Filtered Findings computed property
  readonly filteredFindings = computed(() => {
    let items = this.reviewService.findings();
    const severity = this.selectedSeverity();
    const domain = this.selectedDomain();

    if (severity !== 'All') {
      items = items.filter(f => f.severity.toLowerCase() === severity.toLowerCase());
    }
    if (domain !== 'All') {
      items = items.filter(f => f.domain.toLowerCase() === domain.toLowerCase());
    }
    return items;
  });

  readonly totalFindingsCount = computed(() => this.reviewService.findings().length);

  readonly criticalCount = computed(() =>
    this.reviewService.findings().filter(f => f.severity.toLowerCase() === 'critical').length
  );

  readonly highCount = computed(() =>
    this.reviewService.findings().filter(f => f.severity.toLowerCase() === 'high').length
  );

  readonly mediumCount = computed(() =>
    this.reviewService.findings().filter(f => f.severity.toLowerCase() === 'medium').length
  );

  readonly hasReport = computed(() => this.reviewService.reviewReport() !== null);

  public runReview(): void {
    const id = this.documentId();
    if (!id || id === 'new') {
      console.warn('Cannot run review on unsaved document without valid ID');
      return;
    }
    this.appliedPatchIds.set(new Set());
    this.reviewService.startReviewStream(id);
  }

  public applyPatch(finding: CodeFinding): void {
    this.onApplyPatch.emit({
      problematicCode: finding.problematic_code,
      patchCode: finding.recommended_patch
    });

    this.appliedPatchIds.update(set => {
      const next = new Set(set);
      next.add(finding.id);
      return next;
    });
  }

  public isPatchApplied(findingId: string): boolean {
    return this.appliedPatchIds().has(findingId);
  }

  public setSeverityFilter(severity: FindingSeverity | 'All'): void {
    this.selectedSeverity.set(severity);
  }

  public setDomainFilter(domain: DomainType | 'All'): void {
    this.selectedDomain.set(domain);
  }

  public getScoreColor(score: number): string {
    if (score >= 80) return '#10b981'; // Green / Success
    if (score >= 50) return '#f59e0b'; // Amber / Warning
    return '#ef4444'; // Red / Danger
  }

  public getScoreBg(score: number): string {
    if (score >= 80) return '#ecfdf5';
    if (score >= 50) return '#fffbeb';
    return '#fef2f2';
  }
}
