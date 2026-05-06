import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ISectionsService, CreateSectionRequest, UpdateSectionRequest } from './sections.service.contract';

@Injectable({ providedIn: 'root' })
export class SectionsService implements ISectionsService {
  private readonly http = inject(HttpClient);

  create(request: CreateSectionRequest): Observable<number> {
    return this.http.post<number>('/api/sections', request);
  }

  update(id: number, request: UpdateSectionRequest): Observable<void> {
    return this.http.put('/api/sections/' + id, request).pipe(map(() => undefined));
  }

  delete(id: number): Observable<void> {
    return this.http.delete('/api/sections/' + id).pipe(map(() => undefined));
  }
}
