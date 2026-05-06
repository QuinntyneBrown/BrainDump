import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { ReorderItem } from '../models/reorder-item';

export interface IReorderService {
  reorderSiblings(items: ReorderItem[]): Observable<void>;
}

export const REORDER_SERVICE = new InjectionToken<IReorderService>('REORDER_SERVICE');
