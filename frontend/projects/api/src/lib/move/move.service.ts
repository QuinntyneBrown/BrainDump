import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IMoveService, MoveRequest } from './move.service.contract';

@Injectable({ providedIn: 'root' })
export class MoveServiceImpl implements IMoveService {
  private readonly http = inject(HttpClient);

  move(request: MoveRequest): Observable<void> {
    return this.http.post<void>('/api/move', request);
  }
}
