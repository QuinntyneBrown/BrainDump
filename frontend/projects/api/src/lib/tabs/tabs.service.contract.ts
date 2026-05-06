import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { TabStateDto } from '../models/tab-state.dto';

export interface ITabsService {
  get(): Observable<TabStateDto>;
  put(state: TabStateDto): Observable<void>;
}

export const TABS_SERVICE = new InjectionToken<ITabsService>('TABS_SERVICE');
