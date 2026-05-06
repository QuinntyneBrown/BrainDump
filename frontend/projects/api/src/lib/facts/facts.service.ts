import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { IFactsService, CreateFactRequest, UpdateFactRequest } from './facts.service.contract';

@Injectable({ providedIn: 'root' })
export class FactsService implements IFactsService {
  private readonly http = inject(HttpClient);

  create(request: CreateFactRequest): Observable<number> {
    return this.http.post<number>('/api/facts', request);
  }

  update(id: number, request: UpdateFactRequest): Observable<void> {
    return this.http.put('/api/facts/' + id, request).pipe(map(() => undefined));
  }

  delete(id: number): Observable<void> {
    return this.http.delete('/api/facts/' + id).pipe(map(() => undefined));
  }
}
