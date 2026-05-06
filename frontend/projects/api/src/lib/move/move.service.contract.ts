import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export type MoveKind = 'Folder' | 'Document';

export interface MoveRequest {
  kind: MoveKind;
  id: number;
  targetParentId: number | null;
  position: number;
}

export interface IMoveService {
  move(request: MoveRequest): Observable<void>;
}

export const MOVE_SERVICE = new InjectionToken<IMoveService>('MOVE_SERVICE');
