import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { TreeDto } from '../models/tree.dto';

export interface ITreeService {
  getTree(): Observable<TreeDto>;
}

export const TREE_SERVICE = new InjectionToken<ITreeService>('TREE_SERVICE');
