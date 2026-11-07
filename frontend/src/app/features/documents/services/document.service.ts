import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DocumentListItem,
  Document,
  CreateDocumentRequest,
  UpdateDocumentRequest,
  DocumentAnalysis
} from '../models/document.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/documents';

  getAll(): Observable<DocumentListItem[]> {
    return this.http.get<DocumentListItem[]>(this.baseUrl);
  }

  getById(id: string): Observable<Document> {
    return this.http.get<Document>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateDocumentRequest): Observable<Document> {
    return this.http.post<Document>(this.baseUrl, request);
  }

  update(id: string, request: UpdateDocumentRequest): Observable<Document> {
    return this.http.put<Document>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  uploadFile(file: File): Observable<Document> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<Document>(`${this.baseUrl}/upload`, formData);
  }

  analyze(id: string): Observable<DocumentAnalysis> {
    return this.http.post<DocumentAnalysis>(`${this.baseUrl}/${id}/analysis`, {});
  }
}
