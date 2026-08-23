import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { AuthService } from '../../services/auth/auth.service';
import { TokenService } from '../../services/auth/token.service';

/**
 * Handshake guard:
 * - No access token → login
 * - User already in session → allow
 * - Else call /auth/me
 * - API down → soft allow if token exists (data calls will toast)
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const tokens = inject(TokenService);
  const router = inject(Router);

  if (!tokens.getAccessToken()) {
    void router.navigate(['/login']);
    return false;
  }

  if (auth.getCurrentUser()) {
    return true;
  }

  return auth.loadCurrentUser().pipe(
    map((user) => {
      if (user) {
        return true;
      }
      void router.navigate(['/login'], {
        queryParams: { reason: 'unauthorized' },
      });
      return false;
    }),
    catchError(() => of(!!tokens.getAccessToken())),
  );
};