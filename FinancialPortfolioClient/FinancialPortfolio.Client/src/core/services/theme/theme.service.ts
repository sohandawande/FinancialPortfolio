import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ThemeMode, ResolvedTheme } from '../../models/theme/theme.model';

const STORAGE_KEY = 'fp-theme-mode';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  /** User preference: light | dark | system */
  readonly mode = signal<ThemeMode>(this.readStoredMode());

  /** Actual theme applied to the DOM */
  readonly resolved = computed<ResolvedTheme>(() => {
    const m = this.mode();
    if (m === 'system') {
      return this.getSystemPreference();
    }
    return m;
  });

  private mediaQuery?: MediaQueryList;
  private mediaListener?: (e: MediaQueryListEvent) => void;

  constructor() {
    if (!this.isBrowser) {
      return;
    }

    this.apply(this.resolved());

    // React when OS theme changes (only if mode === system)
    this.mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    this.mediaListener = () => {
      if (this.mode() === 'system') {
        this.apply(this.getSystemPreference());
      }
    };
    this.mediaQuery.addEventListener('change', this.mediaListener);
  }

  setMode(mode: ThemeMode): void {
    this.mode.set(mode);
    if (this.isBrowser) {
      localStorage.setItem(STORAGE_KEY, mode);
    }
    this.apply(this.resolved());
  }

  /** Cycle: light → dark → system → light */
  cycle(): void {
    const order: ThemeMode[] = ['light', 'dark', 'system'];
    const i = order.indexOf(this.mode());
    this.setMode(order[(i + 1) % order.length]);
  }

  private apply(theme: ResolvedTheme): void {
    if (!this.isBrowser) {
      return;
    }
    document.documentElement.setAttribute('data-theme', theme);
    // Bootstrap 5.3 color-mode attribute (optional, helps form-controls)
    document.documentElement.setAttribute('data-bs-theme', theme);
  }

  private getSystemPreference(): ResolvedTheme {
    if (!this.isBrowser) {
      return 'light';
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches
      ? 'dark'
      : 'light';
  }

  private readStoredMode(): ThemeMode {
    if (!this.isBrowser) {
      return 'system';
    }
    const v = localStorage.getItem(STORAGE_KEY);
    if (v === 'light' || v === 'dark' || v === 'system') {
      return v;
    }
    return 'system';
  }
}