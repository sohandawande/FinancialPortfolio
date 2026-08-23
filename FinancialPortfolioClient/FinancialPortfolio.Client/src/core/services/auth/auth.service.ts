import { Injectable, inject } from '@angular/core';
import { HttpBackend, HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, of, map, switchMap, catchError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/common/api-response';
import { LoginRequest } from '../../models/auth/login-request';
import { LoginResponse } from '../../models/auth/login-response';
import { RegisterRequest } from '../../models/auth/register-request';
import { CurrentUser } from '../../models/auth/current-user';
import { TokenService } from './token.service';
import { SessionService } from './session.service';
import { RealtimeService } from '../realtime/realtime.service';
import { ForgotPasswordRequest } from '../../models/auth/forgot-password-request';
import { ResetPasswordRequest } from '../../models/auth/reset-password-request';
import { ChangePasswordRequest } from '../../models/auth/change-password-request';

/**
 * Auth API → api/Auth/*
 * Login body uses loginId (email | username | userCode).
 * Register body uses userName (UserCode is generated on server).
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokenService = inject(TokenService);
  private readonly sessionService = inject(SessionService);
  private readonly realtime = inject(RealtimeService);

  private readonly apiUrl = `${environment.apiUrl}/auth`;

  /** Bypasses interceptors — used only for token refresh. */
  private readonly rawHttp = new HttpClient(inject(HttpBackend));

  login(request: LoginRequest, rememberMe = true): Observable<boolean> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, request).pipe(
      switchMap((res) => {
        if (!res?.success || !res.data?.accessToken) {
          return of(false);
        }

        this.tokenService.setTokens(
          res.data.accessToken,
          res.data.refreshToken,
          rememberMe,
        );

        return this.loadCurrentUser().pipe(
          map((user) => {
            if (user?.roles?.includes('Admin')) {
              void this.realtime.start();
            }
            return true;
          }),
          catchError(() => of(true)),
        );
      }),
      catchError(() => of(false)),
    );
  }

  register(request: RegisterRequest): Observable<{ success: boolean; message: string }> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/register`, request).pipe(
      map((res) => ({
        success: !!res?.success,
        message:
          res?.message ||
          (res?.success
            ? 'Registered successfully. Please wait for admin approval.'
            : 'Registration failed'),
      })),
      catchError((err) => {
        const message =
          err?.error?.message ||
          err?.error?.errors?.[0] ||
          'Registration failed. Please try again.';
        return of({ success: false, message });
      }),
    );
  }

  loadCurrentUser(): Observable<CurrentUser | null> {
    return this.http.get<ApiResponse<CurrentUser> | CurrentUser>(`${this.apiUrl}/me`).pipe(
      map((res) => {
        const user = ((res as ApiResponse<CurrentUser>)?.data ?? res) as CurrentUser;
        if (user?.identityUserId || user?.email) {
          this.sessionService.setUser(user);
          return user;
        }
        return null;
      }),
      catchError(() => of(null)),
    );
  }

  clearSession(): void {
    void this.realtime.stop();
    this.tokenService.clear();
    this.sessionService.clearUser();
  }

  logout(): void {
    const token = this.tokenService.getAccessToken();

    if (token) {
      this.http.post(`${this.apiUrl}/logout`, {}).subscribe({ error: () => {} });
    }

    this.clearSession();
    this.router.navigate(['/login']);
  }

  /**
   * Exchange current access + refresh tokens for a new pair.
   * Uses rawHttp so the call never goes through the auth interceptor.
   */
  refreshTokens(): Observable<boolean> {
    const accessToken = this.tokenService.getAccessToken();
    const refreshToken = this.tokenService.getRefreshToken();

    if (!accessToken || !refreshToken) {
      return of(false);
    }

    const body = { accessToken, refreshToken };
    const rememberMe = this.tokenService.isRememberMe();

    return this.rawHttp
      .post<ApiResponse<LoginResponse>>(`${this.apiUrl}/refresh-token`, body)
      .pipe(
        map((res) => {
          if (!res?.success || !res.data?.accessToken || !res.data?.refreshToken) {
            return false;
          }
          this.tokenService.setTokens(
            res.data.accessToken,
            res.data.refreshToken,
            rememberMe,
          );
          return true;
        }),
        catchError(() => of(false)),
      );
  }

  isAuthenticated(): boolean {
    return this.tokenService.isLoggedIn();
  }

  getCurrentUser(): CurrentUser | null {
    return this.sessionService.getUser();
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return !!user?.roles?.includes(role);
  }

  /** GET api/Auth/check-email?email= */
  checkEmailAvailable(email: string): Observable<boolean> {
    return this.http
      .get<ApiResponse<boolean>>(`${this.apiUrl}/check-email`, {
        params: { email },
      })
      .pipe(
        map((res) => !!res?.data),
        catchError(() => of(true)),
      );
  }

  /** GET api/Auth/check-username?userName= */
  checkUserNameAvailable(userName: string): Observable<boolean> {
    return this.http
      .get<ApiResponse<boolean>>(`${this.apiUrl}/check-username`, {
        params: { userName },
      })
      .pipe(
        map((res) => !!res?.data),
        catchError(() => of(true)),
      );
  }

  forgotPassword(
    request: ForgotPasswordRequest,
  ): Observable<{ success: boolean; message: string }> {
    return this.http
      .post<ApiResponse<boolean>>(`${this.apiUrl}/forgot-password`, request)
      .pipe(
        map((res) => ({
          success: !!res?.success,
          message:
            res?.message ||
            (res?.success
              ? 'If an account exists, a reset link has been sent.'
              : 'Unable to process request.'),
        })),
        catchError((err) =>
          of({
            success: false,
            message: err?.error?.message || 'Unable to send reset email.',
          }),
        ),
      );
  }

  resetPassword(
    request: ResetPasswordRequest,
  ): Observable<{ success: boolean; message: string }> {
    return this.http
      .post<ApiResponse<boolean>>(`${this.apiUrl}/reset-password`, request)
      .pipe(
        map((res) => ({
          success: !!res?.success,
          message: res?.message || 'Password reset successfully.',
        })),
        catchError((err) =>
          of({
            success: false,
            message:
              err?.error?.message || 'Invalid or expired reset link.',
          }),
        ),
      );
  }

  changePassword(
    request: ChangePasswordRequest,
  ): Observable<{ success: boolean; message: string }> {
    return this.http
      .post<ApiResponse<boolean>>(`${this.apiUrl}/change-password`, request)
      .pipe(
        map((res) => ({
          success: !!res?.success,
          message: res?.message || 'Password changed successfully.',
        })),
        catchError((err) =>
          of({
            success: false,
            message: err?.error?.message || 'Failed to change password.',
          }),
        ),
      );
  }
}