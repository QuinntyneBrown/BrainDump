import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export interface IAuthService {
  login(email: string, password: string): Observable<void>;
  logout(): void;
  isAuthenticated(): boolean;
  getAccessToken(): string | null;
}

export const AUTH_SERVICE = new InjectionToken<IAuthService>('AUTH_SERVICE');
