import {
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import {
  BehaviorSubject,
  catchError,
  filter,
  switchMap,
  take,
  throwError,
} from 'rxjs';
import { Router } from '@angular/router';

import { AuthService } from '../../services/auth/auth.service';
import { TokenService } from '../../services/auth/token.service';
import { SessionExpiryService } from '../../services/auth/session-expiry.service';

/** Endpoints that must never trigger a token refresh (prevents infinite loops). */
const SKIP_REFRESH_URLS = [
  '/auth/login',
  '/auth/register',
  '/auth/refresh-token',
  '/auth/forgot-password',
  '/auth/reset-password',
];

let isRefreshing = false;

/** null = in progress / idle, true = ok, false = failed */
const refreshResult$ = new BehaviorSubject<boolean | null>(null);

function shouldSkipRefresh(url: string): boolean {
  return SKIP_REFRESH_URLS.some((path) => url.includes(path));
}

function withBearer(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
}

/**
 * 1. Attach Bearer access token
 * 2. On 401 → single-flight refresh → retry
 * 3. Refresh fail → session expired modal (countdown logout)
 * 4. status 0 (API down) → toast only, keep session
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const authService = inject(AuthService);
  const sessionExpiry = inject(SessionExpiryService);
  const router = inject(Router);

  const accessToken = tokenService.getAccessToken();
  const authReq = accessToken ? withBearer(req, accessToken) : req;

  return next(authReq).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      // API down / offline / CORS — do NOT logout
      if (error.status === 0) {
        sessionExpiry.notifyApiUnavailable();
        return throwError(() => error);
      }

      if (error.status !== 401) {
        return throwError(() => error);
      }

      // Public auth endpoints or no refresh token
      if (shouldSkipRefresh(req.url) || !tokenService.getRefreshToken()) {
        sessionExpiry.startSessionExpired(router.url);
        return throwError(() => error);
      }

      // Wait for in-flight refresh
      if (isRefreshing) {
        return refreshResult$.pipe(
          filter((result): result is boolean => result !== null),
          take(1),
          switchMap((success) => {
            if (!success) {
              return throwError(() => error);
            }
            const newToken = tokenService.getAccessToken();
            if (!newToken) {
              sessionExpiry.startSessionExpired(router.url);
              return throwError(() => error);
            }
            return next(withBearer(req, newToken));
          }),
        );
      }

      // Start shared refresh
      isRefreshing = true;
      refreshResult$.next(null);

      return authService.refreshTokens().pipe(
        switchMap((success) => {
          isRefreshing = false;
          refreshResult$.next(success);

          if (!success) {
            sessionExpiry.startSessionExpired(router.url);
            return throwError(() => error);
          }

          const newToken = tokenService.getAccessToken();
          if (!newToken) {
            sessionExpiry.startSessionExpired(router.url);
            return throwError(() => error);
          }

          return next(withBearer(req, newToken));
        }),
        catchError((refreshError) => {
          isRefreshing = false;
          refreshResult$.next(false);
          sessionExpiry.startSessionExpired(router.url);
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};