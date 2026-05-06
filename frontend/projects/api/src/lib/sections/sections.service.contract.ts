import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export interface CreateSectionRequest {
  documentId: number;
  parentId: number | null;
  title: string;
  position: number;
}

export interface UpdateSectionRequest {
  parentId: number | null;
  title: string;
  position: number;
}

export interface ISectionsService {
  create(request: CreateSectionRequest): Observable<number>;
  update(id: number, request: UpdateSectionRequest): Observable<void>;
  delete(id: number): Observable<void>;
}

export const SECTIONS_SERVICE = new InjectionToken<ISectionsService>('SECTIONS_SERVICE');
