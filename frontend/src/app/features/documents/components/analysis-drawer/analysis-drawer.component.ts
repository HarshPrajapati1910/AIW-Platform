import { Component, input, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DocumentAnalysis, CandidateTask } from '../../models/document.model';

export type InsightTab = 'all' | 'summary' | 'tasks' | 'decisions' | 'issues';

@Component({
  selector: 'app-analysis-drawer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analysis-drawer.component.html',
  styleUrls: ['./analysis-drawer.component.scss']
})
export class AnalysisDrawerComponent {
  readonly analysis = input<DocumentAnalysis | null>(null);
  readonly isAnalyzing = input<boolean>(false);
  readonly errorMessage = input<string | null>(null);

  readonly closeDrawer = output<void>();
  readonly reanalyze = output<void>();

  readonly activeTab = signal<InsightTab>('all');
  readonly copiedState = signal<boolean>(false);

  // [Start] Accept Task into Document Checklist Implementation
  readonly taskAccepted = output<CandidateTask>();
  readonly acceptedTaskTitles = signal<Set<string>>(new Set<string>());
  // [End] Accept Task into Document Checklist

  readonly hasData = computed(() => {
    const data = this.analysis();
    if (!data) return false;
    return !!(
      data.summary ||
      (data.keyPoints && data.keyPoints.length > 0) ||
      (data.candidateTasks && data.candidateTasks.length > 0) ||
      (data.candidateDecisions && data.candidateDecisions.length > 0) ||
      (data.candidateIssues && data.candidateIssues.length > 0)
    );
  });

  setTab(tab: InsightTab): void {
    this.activeTab.set(tab);
  }

  onClose(): void {
    this.closeDrawer.emit();
  }

  onReanalyze(): void {
    this.reanalyze.emit();
  }

  copySummary(): void {
    const data = this.analysis();
    if (!data || !data.summary) return;
    
    let textToCopy = `AI SUMMARY (${data.modelUsed}):\n${data.summary}\n\nKEY TAKEAWAYS:\n`;
    if (data.keyPoints) {
      textToCopy += data.keyPoints.map(kp => `- ${kp}`).join('\n');
    }

    navigator.clipboard.writeText(textToCopy).then(() => {
      this.copiedState.set(true);
      setTimeout(() => this.copiedState.set(false), 2000);
    });
  }

  // [Start] Accept Task into Document Checklist
  isTaskAccepted(title: string): boolean {
    return this.acceptedTaskTitles().has(title);
  }

  onAcceptTask(task: CandidateTask): void {
    if (this.isTaskAccepted(task.title)) return;

    this.acceptedTaskTitles.update((prevSet) => {
      const nextSet = new Set(prevSet);
      nextSet.add(task.title);
      return nextSet;
    });

    this.taskAccepted.emit(task);
  }
  // [End] Accept Task into Document Checklist
}
