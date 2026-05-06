import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WorkspaceDto } from '../models/workspace.dto';
import { IWorkspaceService } from './workspace.service.contract';

@Injectable({ providedIn: 'root' })
export class WorkspaceService implements IWorkspaceService {
  private readonly http = inject(HttpClient);

  get(): Observable<WorkspaceDto> {
    return this.http.get<WorkspaceDto>('/api/workspace');
  }
}
