import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { AUTH_SERVICE } from './auth.service.contract';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AUTH_SERVICE);
  const token = authService.getAccessToken();

  if (token && req.url.startsWith('/api') && !req.url.startsWith('/api/auth/')) {
    return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
  }

  return next(req);
};
