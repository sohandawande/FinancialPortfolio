import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { PageHeader } from '../../../layout/components/page-header/page-header';
import { FpModal } from '../../../layout/components/fp-modal/fp-modal';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';
import { UserDetail } from '../../../core/models/user/user-detail.model';
import { UserService } from '../../../core/services/user/user.service';
import { ToastService } from '../../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../../core/services/confirm-modal/confirm-modal.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { ASSIGNABLE_ROLES } from '../../../core/constants/user/role.constants';

const LOG_FILE = 'user-details.ts';

@Component({
  selector: 'app-user-details',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeader, FpModal],
  templateUrl: './user-details.html',
  styleUrl: './user-details.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly userService = inject(UserService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly logger = inject(ClientLoggerService);
  private readonly auth = inject(AuthService);

  readonly user = signal<UserDetail | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly busy = signal(false);

  // Roles modal
  readonly showRoles = signal(false);
  readonly selectedRoles = signal<string[]>([]);
  readonly savingRoles = signal(false);

  readonly assignableRoles = ASSIGNABLE_ROLES;

  readonly isSelf = computed(() => {
    const u = this.user();
    const me = this.auth.getCurrentUser()?.identityUserId ?? '';
    return !!u && !!me && u.identityUserId === me;
  });

  readonly initials = computed(() => {
    const u = this.user();
    if (!u) return '?';
    const parts = (u.fullName || u.userName || '').trim().split(/\s+/);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return (parts[0]?.[0] ?? '?').toUpperCase();
  });

  readonly headerActions = computed<PageHeaderAction[]>(() => {
    const u = this.user();
    const actions: PageHeaderAction[] = [
      {
        id: 'back',
        label: 'Back to manage',
        icon: 'bi-arrow-left',
        color: 'outline-secondary',
      },
    ];

    if (!u || this.isSelf()) return actions;

    actions.push({
      id: 'roles',
      label: 'Change roles',
      icon: 'bi-person-gear',
      color: 'primary',
      disabled: this.busy() || this.savingRoles(),
    });

    if (u.isActive) {
      actions.push({
        id: 'deactivate',
        label: 'Deactivate',
        icon: 'bi-toggle-off',
        color: 'outline-danger',
        disabled: this.busy(),
      });
    } else {
      actions.push({
        id: 'activate',
        label: 'Activate',
        icon: 'bi-toggle-on',
        color: 'success',
        disabled: this.busy(),
      });
    }

    return actions;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.notFound.set(true);
      this.loading.set(false);
      return;
    }
    this.load(id);
  }

  load(identityUserId: string): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.userService.getUserById(identityUserId).subscribe({
      next: (data) => {
        this.loading.set(false);
        if (!data) {
          this.notFound.set(true);
          this.user.set(null);
          return;
        }
        this.user.set(data);
      },
      error: (err) => {
        this.loading.set(false);
        this.notFound.set(true);
        this.logger.error('Load user detail failed', err, LOG_FILE, 'load');
        this.toast.error('Failed to load user');
      },
    });
  }

  onHeaderAction(id: string): void {
    if (id === 'back') {
      void this.router.navigate(['/users/manage']);
      return;
    }
    if (id === 'roles') this.openRoles();
    if (id === 'activate') void this.activate();
    if (id === 'deactivate') void this.deactivate();
  }

  back(): void {
    void this.router.navigate(['/users/manage']);
  }

  // ── Roles modal ──────────────────────────────────────────────
  openRoles(): void {
    const u = this.user();
    if (!u || this.isSelf()) return;
    this.selectedRoles.set([...(u.roles || [])]);
    this.showRoles.set(true);
  }

  closeRoles(): void {
    if (this.savingRoles()) return;
    this.showRoles.set(false);
  }

  isRoleSelected(role: string): boolean {
    return this.selectedRoles().includes(role);
  }

  toggleRole(role: string, checked: boolean): void {
    const current = this.selectedRoles();
    if (checked) {
      if (!current.includes(role)) this.selectedRoles.set([...current, role]);
    } else {
      this.selectedRoles.set(current.filter((r) => r !== role));
    }
  }

  saveRoles(): void {
    const u = this.user();
    const roles = this.selectedRoles();
    if (!u || roles.length === 0) return;

    this.savingRoles.set(true);
    this.userService.assignRoles(u.identityUserId, roles).subscribe({
      next: (ok) => {
        this.savingRoles.set(false);
        if (ok) {
          this.toast.success('Roles updated');
          this.closeRoles();
          this.load(u.identityUserId);
        } else {
          this.toast.error('Failed to update roles');
        }
      },
      error: (err) => {
        this.savingRoles.set(false);
        this.toast.error('Failed to update roles');
        this.logger.error('Assign roles failed', err, LOG_FILE, 'saveRoles');
      },
    });
  }

  // ── Activate / Deactivate ────────────────────────────────────
  async activate(): Promise<void> {
    const u = this.user();
    if (!u || this.isSelf() || this.busy()) return;

    const ok = await this.confirm.open({
      title: 'Activate user',
      message: `Activate ${u.fullName}?`,
      confirmText: 'Activate',
      confirmColor: 'success',
    });
    if (!ok) return;

    this.busy.set(true);
    this.userService.activateUser(u.identityUserId).subscribe({
      next: (success) => {
        this.busy.set(false);
        if (success) {
          this.toast.success('User activated');
          this.load(u.identityUserId);
        } else {
          this.toast.error('Failed to activate');
        }
      },
      error: (err) => {
        this.busy.set(false);
        this.toast.error('Failed to activate');
        this.logger.error('Activate failed', err, LOG_FILE, 'activate');
      },
    });
  }

  async deactivate(): Promise<void> {
    const u = this.user();
    if (!u || this.isSelf() || this.busy()) return;

    const ok = await this.confirm.open({
      title: 'Deactivate user',
      message: `Deactivate ${u.fullName}?`,
      confirmText: 'Deactivate',
      confirmColor: 'danger',
    });
    if (!ok) return;

    this.busy.set(true);
    this.userService.deactivateUser(u.identityUserId).subscribe({
      next: (success) => {
        this.busy.set(false);
        if (success) {
          this.toast.success('User deactivated');
          this.load(u.identityUserId);
        } else {
          this.toast.error('Failed to deactivate');
        }
      },
      error: (err) => {
        this.busy.set(false);
        this.toast.error('Failed to deactivate');
        this.logger.error('Deactivate failed', err, LOG_FILE, 'deactivate');
      },
    });
  }
}
