import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { WorkspaceDto } from '../models/workspace.dto';

export interface IWorkspaceService {
  get(): Observable<WorkspaceDto>;
}

export const WORKSPACE_SERVICE = new InjectionToken<IWorkspaceService>('WORKSPACE_SERVICE');
