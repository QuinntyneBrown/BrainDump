import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BacklinkDto } from '../models/backlink.dto';
import { TreeDto } from '../models/tree.dto';
import {
  CreateDocumentRequest,
  IDocumentsService,
  UpdateDocumentRequest,
} from './documents.service.contract';

@Injectable({ providedIn: 'root' })
export class DocumentsService implements IDocumentsService {
  private readonly http = inject(HttpClient);

  create(request: CreateDocumentRequest): Observable<{ id: number }> {
    return this.http.post<{ id: number }>('/api/documents', request);
  }

  update(id: number, request: UpdateDocumentRequest): Observable<void> {
    return this.http.put<void>(`/api/documents/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/documents/${id}`);
  }

  getTree(documentId: number): Observable<TreeDto> {
    return this.http.get<TreeDto>(`/api/documents/${documentId}/tree`);
  }

  getBacklinks(documentId: number): Observable<readonly BacklinkDto[]> {
    return this.http.get<readonly BacklinkDto[]>(`/api/documents/${documentId}/backlinks`);
  }
}
