import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route: ActivatedRouteSnapshot) => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const user = auth.user();

    if (!user) {
      router.navigate(['/auth/login']);
      return false;
    }

    if (allowedRoles.includes(user.role)) return true;

    router.navigate(['/']);
    return false;
  };
};
