import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DocumentService } from '../../services/document.service';
import { DocumentListItem } from '../../models/document.model';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './document-list.component.html',
  styleUrls: ['./document-list.component.scss']
})
export class DocumentListComponent implements OnInit {
  private readonly documentService = inject(DocumentService);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);

  readonly documents = signal<DocumentListItem[]>([]);
  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string | null>(null);
  readonly searchQuery = signal<string>('');
  readonly isUploading = signal<boolean>(false);
  readonly uploadError = signal<string | null>(null);
  readonly isDragOver = signal<boolean>(false);

  readonly filteredDocuments = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const list = this.documents();
    if (!query) return list;

    return list.filter(
      (doc) =>
        doc.title.toLowerCase().includes(query) ||
        doc.preview.toLowerCase().includes(query)
    );
  });

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.documentService.getAll().subscribe({
      next: (docs) => {
        this.documents.set(docs || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching documents', err);
        let msg = 'Failed to connect to backend server.';
        if (err?.error?.message) {
          msg = err.error.message;
        } else if (err?.message?.includes('Unexpected token') || (typeof err?.error === 'string' && err.error?.includes('<!doctype'))) {
          msg = 'Backend API server (http://localhost:5200) is not running or endpoint returned index.html. Please start the backend via `dotnet run` or verify port 5200.';
        } else if (err?.status === 0) {
          msg = 'Backend API server (http://localhost:5200) is offline.';
        }
        this.errorMessage.set(msg);
        this.isLoading.set(false);
      }
    });
  }

  openDocument(id: string): void {
    this.router.navigate(['/documents', id]);
  }

  createNew(): void {
    this.router.navigate(['/documents/new']);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.uploadFile(input.files[0]);
    }
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      this.uploadFile(event.dataTransfer.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver.set(false);
  }

  uploadFile(file: File): void {
    const allowedExtensions = ['.pdf', '.txt'];
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!allowedExtensions.includes(ext)) {
      this.uploadError.set('Only .pdf and .txt files are supported.');
      return;
    }

    this.isUploading.set(true);
    this.uploadError.set(null);

    this.documentService.uploadFile(file).subscribe({
      next: (createdDoc) => {
        this.isUploading.set(false);
        this.router.navigate(['/documents', createdDoc.id]);
      },
      error: (err) => {
        console.error('Upload failed', err);
        this.uploadError.set(
          err?.error?.message || 'Failed to upload document file.'
        );
        this.isUploading.set(false);
      }
    });
  }

  deleteDocument(event: Event, id: string, title: string): void {
    event.stopPropagation();
    if (confirm(`Are you sure you want to delete "${title}"?`)) {
      this.documentService.delete(id).subscribe({
        next: () => {
          this.documents.update((list) => list.filter((doc) => doc.id !== id));
        },
        error: (err) => {
          alert(err?.error?.message || 'Failed to delete document.');
        }
      });
    }
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    const now = new Date();
    const diffHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

    if (diffHours < 24) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
    return date.toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
