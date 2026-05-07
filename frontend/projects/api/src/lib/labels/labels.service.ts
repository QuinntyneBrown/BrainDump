import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ILabelsService } from './labels.service.contract';

@Injectable({ providedIn: 'root' })
export class LabelsService implements ILabelsService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<readonly string[]> {
    return this.http.get<readonly string[]>('/api/labels');
  }

  setForDocument(documentId: number, labels: readonly string[]): Observable<void> {
    return this.http.put<void>(`/api/documents/${documentId}/labels`, { labels });
  }
}
