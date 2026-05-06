import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export interface CreateFactRequest {
  sectionId: number;
  text: string;
  position: number;
}

export interface UpdateFactRequest {
  sectionId: number;
  text: string;
  position: number;
}

export interface IFactsService {
  create(request: CreateFactRequest): Observable<number>;
  update(id: number, request: UpdateFactRequest): Observable<void>;
  delete(id: number): Observable<void>;
}

export const FACTS_SERVICE = new InjectionToken<IFactsService>('FACTS_SERVICE');
