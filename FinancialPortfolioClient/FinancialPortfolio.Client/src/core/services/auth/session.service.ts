import { Injectable, signal, computed } from '@angular/core';
import { CurrentUser } from '../../models/auth/current-user';
import { AUTH_CONSTANTS } from '../../constants/auth/auth.constants';

@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly userSignal = signal<CurrentUser | null>(this.readStorage());

  readonly user = this.userSignal.asReadonly();
  readonly isLoggedIn = computed(() => !!this.userSignal());

  setUser(user: CurrentUser): void {
    localStorage.setItem(AUTH_CONSTANTS.USER_KEY, JSON.stringify(user));
    this.userSignal.set(user);
  }

  getUser(): CurrentUser | null {
    return this.userSignal();
  }

  clearUser(): void {
    localStorage.removeItem(AUTH_CONSTANTS.USER_KEY);
    this.userSignal.set(null);
  }

  private readStorage(): CurrentUser | null {
    const raw = localStorage.getItem(AUTH_CONSTANTS.USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }
}