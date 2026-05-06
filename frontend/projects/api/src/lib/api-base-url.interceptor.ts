import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { API_BASE_URL } from './api-base-url';

// Prepends the configured API base URL to relative /api/* requests.
// In dev the base URL is empty and the Angular dev-server proxy handles routing.
export const apiBaseUrlInterceptor: HttpInterceptorFn = (req, next) => {
  const baseUrl = inject(API_BASE_URL);

  if (!baseUrl || !req.url.startsWith('/api')) {
    return next(req);
  }

  return next(req.clone({ url: `${baseUrl}${req.url}` }));
};
