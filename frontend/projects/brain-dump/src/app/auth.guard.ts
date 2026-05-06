import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AUTH_SERVICE } from 'api';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AUTH_SERVICE);
  const router = inject(Router);

  if (authService.isAuthenticated()) return true;
  return router.createUrlTree(['/login']);
};
