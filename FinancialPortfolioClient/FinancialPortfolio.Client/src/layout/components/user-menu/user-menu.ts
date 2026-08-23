import { Component, inject, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../../core/services/auth/auth.service';
import { ConfirmModalService } from '../../../core/services/confirm-modal/confirm-modal.service';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './user-menu.html',
  styleUrl: './user-menu.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserMenu {
  private readonly auth = inject(AuthService);
  private readonly confirm = inject(ConfirmModalService);

  readonly user = computed(() => this.auth.getCurrentUser());

  readonly displayName = computed(() => {
    const u = this.user();
    return u?.fullName?.trim() || u?.userName || u?.email || 'User';
  });

  readonly userCode = computed(() => this.user()?.userCode ?? '');
  readonly email = computed(() => this.user()?.email ?? '');
  readonly rolesLabel = computed(() => {
    const roles = this.user()?.roles ?? [];
    return roles.length ? roles.join(', ') : '—';
  });

  readonly initials = computed(() => {
    const name = this.displayName();
    const parts = name
      .trim()
      .split(/[\s@._-]+/)
      .filter(Boolean);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.slice(0, 2).toUpperCase();
  });

  /**
   * Close any open Bootstrap dropdown so the menu does not remain
   * visible behind the confirm modal (z-index stacking).
   */
  private closeOpenDropdowns(): void {
    // Bootstrap bundle is loaded via angular.json scripts → window.bootstrap
    const bs = (window as any).bootstrap;
    if (!bs?.Dropdown) return;

    document.querySelectorAll<HTMLElement>('.dropdown-menu.show').forEach((menu) => {
      const toggle = menu.previousElementSibling as HTMLElement | null;
      if (toggle) {
        bs.Dropdown.getInstance(toggle)?.hide();
      }
    });
  }

  async logout(): Promise<void> {
    this.closeOpenDropdowns();

    const ok = await this.confirm.open({
      title: 'Sign out',
      message: 'Are you sure you want to sign out?',
      confirmText: 'Sign out',
      confirmColor: 'danger',
    });

    if (ok) {
      this.auth.logout();
    }
  }
}