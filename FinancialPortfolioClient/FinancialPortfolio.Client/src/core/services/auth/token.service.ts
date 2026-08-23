import { Injectable } from '@angular/core';
import { AUTH_CONSTANTS } from '../../constants/auth/auth.constants';

@Injectable({ providedIn: 'root' })
export class TokenService {
  setAccessToken(token: string, rememberMe = true): void {
    if (rememberMe) {
      localStorage.setItem(AUTH_CONSTANTS.TOKEN_KEY, token);
      sessionStorage.removeItem(AUTH_CONSTANTS.TOKEN_KEY);
    } else {
      sessionStorage.setItem(AUTH_CONSTANTS.TOKEN_KEY, token);
      localStorage.removeItem(AUTH_CONSTANTS.TOKEN_KEY);
    }
  }

  getAccessToken(): string | null {
    return (
      localStorage.getItem(AUTH_CONSTANTS.TOKEN_KEY) ||
      sessionStorage.getItem(AUTH_CONSTANTS.TOKEN_KEY)
    );
  }

  setRefreshToken(token: string, rememberMe = true): void {
    if (rememberMe) {
      localStorage.setItem(AUTH_CONSTANTS.REFRESH_TOKEN_KEY, token);
      sessionStorage.removeItem(AUTH_CONSTANTS.REFRESH_TOKEN_KEY);
    } else {
      sessionStorage.setItem(AUTH_CONSTANTS.REFRESH_TOKEN_KEY, token);
      localStorage.removeItem(AUTH_CONSTANTS.REFRESH_TOKEN_KEY);
    }
  }

  getRefreshToken(): string | null {
    return (
      localStorage.getItem(AUTH_CONSTANTS.REFRESH_TOKEN_KEY) ||
      sessionStorage.getItem(AUTH_CONSTANTS.REFRESH_TOKEN_KEY)
    );
  }

  /** Persist both tokens using the same storage preference. */
  setTokens(accessToken: string, refreshToken: string, rememberMe = true): void {
    this.setAccessToken(accessToken, rememberMe);
    this.setRefreshToken(refreshToken, rememberMe);
  }

  /** True when access token is stored in localStorage (Remember me). */
  isRememberMe(): boolean {
    return !!localStorage.getItem(AUTH_CONSTANTS.TOKEN_KEY);
  }

  clear(): void {
    localStorage.removeItem(AUTH_CONSTANTS.TOKEN_KEY);
    localStorage.removeItem(AUTH_CONSTANTS.REFRESH_TOKEN_KEY);
    sessionStorage.removeItem(AUTH_CONSTANTS.TOKEN_KEY);
    sessionStorage.removeItem(AUTH_CONSTANTS.REFRESH_TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }
}