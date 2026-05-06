import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TabStateDto } from '../models/tab-state.dto';
import { ITabsService } from './tabs.service.contract';

@Injectable({ providedIn: 'root' })
export class TabsService implements ITabsService {
  private readonly http = inject(HttpClient);

  get(): Observable<TabStateDto> {
    return this.http.get<TabStateDto>('/api/tabs');
  }

  put(state: TabStateDto): Observable<void> {
    return this.http.put<void>('/api/tabs', state);
  }
}
