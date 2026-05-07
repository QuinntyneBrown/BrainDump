import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { RecentDocumentDto } from '../models/recent-document.dto';

export interface IRecentsService {
  recordView(documentId: number): Observable<void>;
  getRecent(limit?: number): Observable<readonly RecentDocumentDto[]>;
}

export const RECENTS_SERVICE = new InjectionToken<IRecentsService>('RECENTS_SERVICE');
