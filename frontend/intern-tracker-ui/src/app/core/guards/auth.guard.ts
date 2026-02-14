import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    const user = authService.getUser();
    const expectedRole = route.data['role'];

    if (expectedRole && user.role !== expectedRole) {
      // Role mismatch, redirect to home or appropriate dashboard
      if (user.role === 'Admin') {
        return router.createUrlTree(['/admin/dashboard']);
      } else {
        return router.createUrlTree(['/intern/dashboard']);
      }
    }
    return true;
  }

  return router.createUrlTree(['/login']);
};
