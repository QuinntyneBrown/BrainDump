import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { IReorderService } from './reorder.service.contract';
import { ReorderItem } from '../models/reorder-item';

@Injectable({ providedIn: 'root' })
export class ReorderService implements IReorderService {
  private readonly http = inject(HttpClient);

  reorderSiblings(items: ReorderItem[]): Observable<void> {
    return this.http.post('/api/reorder', { items }).pipe(map(() => undefined));
  }
}
