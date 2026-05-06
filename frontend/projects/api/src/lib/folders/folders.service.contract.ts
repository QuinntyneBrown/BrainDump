import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export interface CreateFolderRequest {
  parentId: number | null;
  title: string;
  position: number;
}

export interface UpdateFolderRequest {
  title: string;
  position: number;
}

export interface IFoldersService {
  create(request: CreateFolderRequest): Observable<{ id: number }>;
  update(id: number, request: UpdateFolderRequest): Observable<void>;
  delete(id: number): Observable<void>;
}

export const FOLDERS_SERVICE = new InjectionToken<IFoldersService>('FOLDERS_SERVICE');
