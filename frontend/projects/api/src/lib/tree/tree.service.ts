import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ITreeService } from './tree.service.contract';
import { TreeDto } from '../models/tree.dto';

@Injectable({ providedIn: 'root' })
export class TreeService implements ITreeService {
  private readonly http = inject(HttpClient);

  getTree(): Observable<TreeDto> {
    return this.http.get<TreeDto>('/api/tree');
  }
}
