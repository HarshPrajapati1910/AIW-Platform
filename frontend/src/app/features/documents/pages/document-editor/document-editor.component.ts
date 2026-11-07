import { Component, OnInit, inject, signal, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DocumentService } from '../../services/document.service';
import { DocumentAnalysis, CandidateTask } from '../../models/document.model';
import { AnalysisDrawerComponent } from '../../components/analysis-drawer/analysis-drawer.component';
import { ReviewDashboardComponent } from '../../../review-dashboard/components/review-dashboard/review-dashboard.component';

@Component({
  selector: 'app-document-editor',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, AnalysisDrawerComponent, ReviewDashboardComponent],
  templateUrl: './document-editor.component.html',
  styleUrls: ['./document-editor.component.scss']
})
export class DocumentEditorComponent implements OnInit {
  private readonly documentService = inject(DocumentService);
  private readonly router = inject(Router);

  // Route param input
  readonly id = input<string | undefined>();

  readonly title = signal<string>('');
  readonly content = signal<string>('');
  readonly initialTitle = signal<string>('');
  readonly initialContent = signal<string>('');

  readonly isLoading = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly saveSuccessMessage = signal<string | null>(null);

  // AI Analysis state
  readonly isAnalyzing = signal<boolean>(false);
  readonly analysisResult = signal<DocumentAnalysis | null>(null);
  readonly analysisError = signal<string | null>(null);
  readonly isDrawerOpen = signal<boolean>(false);

  // Review Dashboard state
  readonly isReviewOpen = signal<boolean>(false);

  readonly isEditMode = computed(() => !!this.id() && this.id() !== 'new');

  readonly isDirty = computed(() => {
    return (
      this.title() !== this.initialTitle() ||
      this.content() !== this.initialContent()
    );
  });

  readonly wordCount = computed(() => {
    const text = this.content().trim();
    if (!text) return 0;
    return text.split(/\s+/).length;
  });

  readonly charCount = computed(() => this.content().length);

  ngOnInit(): void {
    const docId = this.id();
    if (docId && docId !== 'new') {
      this.loadDocument(docId);
    } else {
      this.title.set('Untitled Document');
      this.initialTitle.set('Untitled Document');
    }
  }

  loadDocument(docId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.documentService.getById(docId).subscribe({
      next: (doc) => {
        this.title.set(doc.title);
        this.content.set(doc.content);
        this.initialTitle.set(doc.title);
        this.initialContent.set(doc.content);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading document', err);
        this.errorMessage.set(
          err?.error?.message || 'Document not found or failed to load.'
        );
        this.isLoading.set(false);
      }
    });
  }

  save(): void {
    if (!this.title().trim()) {
      alert('Please enter a document title.');
      return;
    }

    this.isSaving.set(true);
    this.saveSuccessMessage.set(null);
    this.errorMessage.set(null);

    const request = {
      title: this.title().trim(),
      content: this.content()
    };

    if (this.isEditMode() && this.id()) {
      this.documentService.update(this.id()!, request).subscribe({
        next: (updated) => {
          this.initialTitle.set(updated.title);
          this.initialContent.set(updated.content);
          this.isSaving.set(false);
          this.showSuccessBanner('Document saved successfully.');
        },
        error: (err) => {
          console.error('Error updating document', err);
          this.errorMessage.set(err?.error?.message || 'Failed to save changes.');
          this.isSaving.set(false);
        }
      });
    } else {
      this.documentService.create(request).subscribe({
        next: (created) => {
          this.initialTitle.set(created.title);
          this.initialContent.set(created.content);
          this.isSaving.set(false);
          this.router.navigate(['/documents', created.id]);
        },
        error: (err) => {
          console.error('Error creating document', err);
          this.errorMessage.set(err?.error?.message || 'Failed to create document.');
          this.isSaving.set(false);
        }
      });
    }
  }

  triggerAnalysis(): void {
    if (this.isDirty() || !this.isEditMode()) {
      // First save if unsaved
      if (!this.title().trim()) {
        alert('Please enter a title before analyzing.');
        return;
      }
      this.isSaving.set(true);
      const request = { title: this.title().trim(), content: this.content() };
      
      const saveObs = (this.isEditMode() && this.id())
        ? this.documentService.update(this.id()!, request)
        : this.documentService.create(request);

      saveObs.subscribe({
        next: (saved) => {
          this.initialTitle.set(saved.title);
          this.initialContent.set(saved.content);
          this.isSaving.set(false);
          if (!this.isEditMode()) {
            this.router.navigate(['/documents', saved.id]);
          }
          this.runAiAnalysis(saved.id);
        },
        error: (err) => {
          this.isSaving.set(false);
          alert('Failed to save document prior to analysis.');
        }
      });
    } else {
      this.runAiAnalysis(this.id()!);
    }
  }

  private runAiAnalysis(docId: string): void {
    this.isReviewOpen.set(false); // Close review drawer if open
    this.isDrawerOpen.set(true);
    this.isAnalyzing.set(true);
    this.analysisError.set(null);

    this.documentService.analyze(docId).subscribe({
      next: (res) => {
        this.analysisResult.set(res);
        this.isAnalyzing.set(false);
      },
      error: (err) => {
        console.error('AI Analysis Error', err);
        this.analysisError.set(
          err?.error?.message || 'AI service unavailable or timed out.'
        );
        this.isAnalyzing.set(false);
      }
    });
  }

  closeDrawer(): void {
    this.isDrawerOpen.set(false);
  }

  deleteDocument(): void {
    if (!this.isEditMode() || !this.id()) return;
    if (confirm(`Are you sure you want to delete "${this.title()}"?`)) {
      this.documentService.delete(this.id()!).subscribe({
        next: () => {
          this.router.navigate(['/documents']);
        },
        error: (err) => {
          alert(err?.error?.message || 'Failed to delete document.');
        }
      });
    }
  }

  goBack(): void {
    if (this.isDirty()) {
      if (!confirm('You have unsaved changes. Discard changes and leave?')) {
        return;
      }
    }
    this.router.navigate(['/documents']);
  }

  private showSuccessBanner(msg: string): void {
    this.saveSuccessMessage.set(msg);
    setTimeout(() => this.saveSuccessMessage.set(null), 3000);
  }

  onTaskAccepted(task: CandidateTask): void {
    const currentContent = (this.content() || '').trimEnd();
    const taskLine = `- [ ] ${task.title}`;
    const header = '### Action Items';

    let updatedContent = '';

    if (currentContent.includes(header)) {
      updatedContent = `${currentContent}\n${taskLine}`;
    } else if (currentContent.length > 0) {
      updatedContent = `${currentContent}\n\n${header}\n${taskLine}`;
    } else {
      updatedContent = `${header}\n${taskLine}`;
    }

    this.content.set(updatedContent);
  }

  // Review Dashboard Integration
  toggleReview(): void {
    if (this.isReviewOpen()) {
      this.closeReview();
    } else {
      this.openReview();
    }
  }

  openReview(): void {
    if (this.isDirty() || !this.isEditMode()) {
      if (!this.title().trim()) {
        alert('Please enter a document title before starting the review.');
        return;
      }
      this.isSaving.set(true);
      const request = { title: this.title().trim(), content: this.content() };
      const saveObs = (this.isEditMode() && this.id())
        ? this.documentService.update(this.id()!, request)
        : this.documentService.create(request);

      saveObs.subscribe({
        next: (saved) => {
          this.initialTitle.set(saved.title);
          this.initialContent.set(saved.content);
          this.isSaving.set(false);
          this.isDrawerOpen.set(false);
          if (!this.isEditMode()) {
            this.router.navigate(['/documents', saved.id]);
          }
          this.isReviewOpen.set(true);
        },
        error: (err) => {
          console.error('Failed to save document prior to review', err);
          this.isSaving.set(false);
          alert('Failed to save document prior to review.');
        }
      });
    } else {
      this.isDrawerOpen.set(false);
      this.isReviewOpen.set(true);
    }
  }

  closeReview(): void {
    this.isReviewOpen.set(false);
  }

  handleApplyPatch(patch: { problematicCode: string; patchCode: string }): void {
    const current = this.content();
    if (!patch.problematicCode) {
      alert('Problematic code snippet is empty.');
      return;
    }

    if (current.includes(patch.problematicCode)) {
      this.content.set(current.replace(patch.problematicCode, patch.patchCode));
      this.showSuccessBanner('Surgical patch applied successfully.');
      return;
    }

    // Try normalized match (handling CRLF vs LF line ending discrepancies)
    const normalizedCurrent = current.replace(/\r\n/g, '\n');
    const normalizedTarget = patch.problematicCode.replace(/\r\n/g, '\n');
    const normalizedPatch = patch.patchCode.replace(/\r\n/g, '\n');

    if (normalizedCurrent.includes(normalizedTarget)) {
      this.content.set(normalizedCurrent.replace(normalizedTarget, normalizedPatch));
      this.showSuccessBanner('Surgical patch applied successfully.');
    } else {
      console.warn('Target code chunk not found in active document editor context.');
      alert('Could not locate the exact problematic code snippet in the document content.');
    }
  }
}
