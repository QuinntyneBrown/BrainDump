import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export interface ILabelsService {
  getAll(): Observable<readonly string[]>;
  setForDocument(documentId: number, labels: readonly string[]): Observable<void>;
}

export const LABELS_SERVICE = new InjectionToken<ILabelsService>('LABELS_SERVICE');
