import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RecentDocumentDto } from '../models/recent-document.dto';
import { IRecentsService } from './recents.service.contract';

@Injectable({ providedIn: 'root' })
export class RecentsService implements IRecentsService {
  private readonly http = inject(HttpClient);

  recordView(documentId: number): Observable<void> {
    return this.http.post<void>(`/api/documents/${documentId}/view`, null);
  }

  getRecent(limit = 10): Observable<readonly RecentDocumentDto[]> {
    return this.http.get<readonly RecentDocumentDto[]>(`/api/recent?limit=${limit}`);
  }
}
