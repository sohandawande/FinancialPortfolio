import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { ToastService } from '../toast/toast.service';

/**
 * Session-expired UX:
 * - Modal + 10s countdown → clear session → login (with returnUrl)
 * - API down → toast only (keep tokens)
 */
@Injectable({ providedIn: 'root' })
export class SessionExpiryService {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly visible = signal(false);
  readonly secondsLeft = signal(10);

  private timerId: ReturnType<typeof setInterval> | null = null;
  private handling = false;
  private returnUrl = '/dashboard';

  /** Token invalid / refresh failed */
  startSessionExpired(returnUrl?: string): void {
    if (this.handling) {
      return;
    }

    this.handling = true;
    this.returnUrl = returnUrl || this.router.url || '/dashboard';
    this.secondsLeft.set(10);
    this.visible.set(true);

    this.clearTimer();
    this.timerId = setInterval(() => {
      const next = this.secondsLeft() - 1;
      this.secondsLeft.set(next);
      if (next <= 0) {
        this.forceLogout();
      }
    }, 1000);
  }

  /** User clicks "Sign in now" */
  logoutNow(): void {
    this.forceLogout();
  }

  /** API unreachable (status 0) — do not logout */
  notifyApiUnavailable(): void {
    this.toast.warning('Server is unavailable. Please try again later.');
  }

  private forceLogout(): void {
    this.clearTimer();
    this.visible.set(false);
    this.handling = false;

    this.auth.clearSession();
    void this.router.navigate(['/login'], {
      queryParams: {
        returnUrl: this.returnUrl,
        reason: 'session-expired',
      },
    });
  }

  private clearTimer(): void {
    if (this.timerId) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
  }
}