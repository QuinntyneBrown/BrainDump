import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateFolderRequest, IFoldersService, UpdateFolderRequest } from './folders.service.contract';

@Injectable({ providedIn: 'root' })
export class FoldersService implements IFoldersService {
  private readonly http = inject(HttpClient);

  create(request: CreateFolderRequest): Observable<{ id: number }> {
    return this.http.post<{ id: number }>('/api/folders', request);
  }

  update(id: number, request: UpdateFolderRequest): Observable<void> {
    return this.http.put<void>(`/api/folders/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`/api/folders/${id}`);
  }
}
