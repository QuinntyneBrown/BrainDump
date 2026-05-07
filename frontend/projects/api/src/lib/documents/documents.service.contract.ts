import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { BacklinkDto } from '../models/backlink.dto';
import { TreeDto } from '../models/tree.dto';

export interface CreateDocumentRequest {
  folderId: number | null;
  title: string;
  position: number;
}

export interface UpdateDocumentRequest {
  title: string;
  position: number;
}

export interface IDocumentsService {
  create(request: CreateDocumentRequest): Observable<{ id: number }>;
  update(id: number, request: UpdateDocumentRequest): Observable<void>;
  delete(id: number): Observable<void>;
  getTree(documentId: number): Observable<TreeDto>;
  getBacklinks(documentId: number): Observable<readonly BacklinkDto[]>;
}

export const DOCUMENTS_SERVICE = new InjectionToken<IDocumentsService>('DOCUMENTS_SERVICE');
