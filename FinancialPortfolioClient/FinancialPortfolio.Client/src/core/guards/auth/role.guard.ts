import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';

export const roleGuard = (roles: string[]): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
      router.navigate(['/login']);
      return false;
    }

    const user = auth.getCurrentUser();
    const hasRole = roles.some((r) => user?.roles?.includes(r));

    if (!hasRole) {
      router.navigate(['/access-denied']);
      return false;
    }

    return true;
  };
};