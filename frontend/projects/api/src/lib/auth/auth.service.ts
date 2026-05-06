import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { from, map, Observable, switchMap, tap } from 'rxjs';
import { IAuthService } from './auth.service.contract';

interface AuthorizeResponse {
  code: string;
}

interface TokenResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService implements IAuthService {
  private readonly http = inject(HttpClient);
  private readonly TOKEN_KEY = 'bd_access_token';
  private readonly EXPIRY_KEY = 'bd_token_expiry';

  login(email: string, password: string): Observable<void> {
    const codeVerifier = this.generateCodeVerifier();

    return from(this.computeCodeChallenge(codeVerifier)).pipe(
      switchMap(codeChallenge =>
        this.http.post<AuthorizeResponse>('/api/auth/authorize', {
          email,
          password,
          codeChallenge,
          codeChallengeMethod: 'S256',
        })
      ),
      switchMap(({ code }) =>
        this.http.post<TokenResponse>('/api/auth/token', {
          code,
          codeVerifier,
          grantType: 'authorization_code',
        })
      ),
      tap(response => this.storeToken(response)),
      map(() => undefined)
    );
  }

  logout(): void {
    sessionStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.EXPIRY_KEY);
  }

  isAuthenticated(): boolean {
    const token = sessionStorage.getItem(this.TOKEN_KEY);
    const expiry = sessionStorage.getItem(this.EXPIRY_KEY);
    if (!token || !expiry) return false;
    return Date.now() < parseInt(expiry, 10);
  }

  getAccessToken(): string | null {
    if (!this.isAuthenticated()) return null;
    return sessionStorage.getItem(this.TOKEN_KEY);
  }

  private storeToken(response: TokenResponse): void {
    sessionStorage.setItem(this.TOKEN_KEY, response.accessToken);
    sessionStorage.setItem(this.EXPIRY_KEY, (Date.now() + response.expiresIn * 1000).toString());
  }

  private generateCodeVerifier(): string {
    const array = new Uint8Array(32);
    crypto.getRandomValues(array);
    return this.base64urlEncode(array.buffer);
  }

  private async computeCodeChallenge(verifier: string): Promise<string> {
    const encoded = new TextEncoder().encode(verifier);
    const hash = await crypto.subtle.digest('SHA-256', encoded);
    return this.base64urlEncode(hash);
  }

  private base64urlEncode(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
  }
}
