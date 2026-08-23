import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RealtimeService } from '../../../core/services/realtime/realtime.service';
import { Subscription } from 'rxjs';

import { DataGrid } from '../../../layout/components/table/data-grid/data-grid';
import { PageHeader } from '../../../layout/components/page-header/page-header';
import { FpModal } from '../../../layout/components/fp-modal/fp-modal';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';
import { ManagedUser } from '../../../core/models/user/managed-user.model';
import { UserService } from '../../../core/services/user/user.service';
import { ToastService } from '../../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../../core/services/confirm-modal/confirm-modal.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { ASSIGNABLE_ROLES } from '../../../core/constants/user/role.constants';
import { QueryRequest } from '../../../core/models/query/query-request.model';
import { TableColumn } from '../../../core/models/query/table-column.model';
import { SortDirection } from '../../../core/models/query/sort-direction.enum';

const LOG_FILE = 'manage-users.ts';

@Component({
  selector: 'app-manage-users',
  standalone: true,
  imports: [CommonModule, DataGrid, PageHeader, FpModal],
  templateUrl: './manage-users.html',
  styleUrl: './manage-users.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ManageUsers implements OnInit {
  private readonly userService = inject(UserService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly logger = inject(ClientLoggerService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly realtime = inject(RealtimeService);
  private pendingSub?: Subscription;

  readonly allUsers = signal<ManagedUser[]>([]);
  readonly loading = signal(false);
  readonly pendingCount = signal(0);
  readonly busyId = signal<string | null>(null);

  readonly showRoles = signal(false);
  readonly selectedUser = signal<ManagedUser | null>(null);
  readonly selectedRoles = signal<string[]>([]);
  readonly saving = signal(false);

  readonly assignableRoles = ASSIGNABLE_ROLES;

  readonly query = signal<QueryRequest>({
    pageNumber: 1,
    pageSize: 10,
    globalSearch: '',
    filters: [],
    sorts: [],
  });

  readonly headerActions = computed<PageHeaderAction[]>(() => [
    {
      id: 'pending',
      label: this.pendingCount() > 0 ? `Pending (${this.pendingCount()})` : 'Pending users',
      icon: 'bi-person-plus',
      color: 'warning',
    },
    {
      id: 'refresh',
      label: 'Refresh',
      icon: 'bi-arrow-clockwise',
      color: 'outline-secondary',
      disabled: this.loading() || this.anyBusy(),
      loading: this.loading(),
    },
  ]);

  readonly filtered = computed(() => {
    let list = [...this.allUsers()];
    const q = (this.query().globalSearch || '').trim().toLowerCase();

    if (q) {
      list = list.filter(
        (u) =>
          u.fullName.toLowerCase().includes(q) ||
          u.email.toLowerCase().includes(q) ||
          u.userCode.toLowerCase().includes(q),
      );
    }

    const sorts = this.query().sorts;
    if (sorts?.length) {
      const { field, direction } = sorts[0];
      const dir = direction === SortDirection.Desc ? -1 : 1;
      list.sort((a, b) => {
        const av = String((a as any)[field] ?? '');
        const bv = String((b as any)[field] ?? '');
        return av.localeCompare(bv) * dir;
      });
    }

    return list;
  });

  readonly totalRecords = computed(() => this.filtered().length);

  readonly pageRows = computed(() => {
    const { pageNumber = 1, pageSize = 10 } = this.query();
    const start = (pageNumber - 1) * pageSize;
    return this.filtered().slice(start, start + pageSize);
  });

  readonly columns = signal<TableColumn<ManagedUser>[]>([
    {
      key: 'userCode',
      header: 'Code',
      sortable: true,
      filterable: true,
      width: '110px',
    },
    {
      key: 'fullName',
      header: 'Name',
      sortable: true,
      filterable: true,
      formatter: (row) =>
        this.isSelf(row)
          ? `${row.fullName} <span class="badge text-bg-primary ms-1">You</span>`
          : row.fullName,
    },
    {
      key: 'email',
      header: 'Email',
      sortable: true,
      filterable: true,
    },
    {
      key: 'roles',
      header: 'Roles',
      formatter: (row) =>
        (row.roles || [])
          .map((r) => `<span class="badge text-bg-secondary me-1">${r}</span>`)
          .join('') || '—',
    },
    {
      key: 'isActive',
      header: 'Status',
      width: '100px',
      formatter: (row) =>
        row.isActive
          ? `<span class="badge rounded-pill stock-active-yes">Active</span>`
          : `<span class="badge rounded-pill stock-active-no">Inactive</span>`,
    },
    {
      key: 'actions',
      header: 'Actions',
      type: 'actions',
      canToggle: false,
      width: '140px',
      actions: [
        {
          icon: 'bi-eye',
          label: 'View',
          color: 'outline-secondary',
          click: (row) => void this.router.navigate(['/users/manage', row.identityUserId]),
        },
        {
          icon: 'bi-person-gear',
          label: 'Roles',
          color: 'outline-primary',
          disabled: (row) => this.isSelf(row) || this.anyBusy(),
          click: (row) => this.openRoles(row),
        },
        {
          icon: 'bi-toggle-on',
          label: 'Activate',
          color: 'outline-success',
          visible: (row) => !row.isActive && !this.isSelf(row),
          disabled: () => this.anyBusy(),
          click: (row) => void this.activate(row),
        },
        {
          icon: 'bi-toggle-off',
          label: 'Deactivate',
          color: 'outline-danger',
          visible: (row) => row.isActive && !this.isSelf(row),
          disabled: () => this.anyBusy(),
          click: (row) => void this.deactivate(row),
        },
      ],
    },
  ]);

  ngOnInit(): void {
    this.load();
    this.loadPendingCount();
    this.pendingSub = this.realtime.onPendingUserCreated.subscribe(() => {
      this.loadPendingCount();
    });
  }

  ngOnDestroy(): void {
    this.pendingSub?.unsubscribe();
  }

  isSelf(user: ManagedUser): boolean {
    const id = this.auth.getCurrentUser()?.identityUserId ?? '';
    return !!id && user.identityUserId === id;
  }

  isBusy(id: string): boolean {
    return this.busyId() === id;
  }

  anyBusy(): boolean {
    return this.busyId() !== null || this.saving();
  }

  onHeaderAction(id: string): void {
    if (id === 'refresh') {
      this.load();
      this.loadPendingCount();
    }
    if (id === 'pending') void this.router.navigate(['/users/pending']);
  }

  onQueryChange(q: QueryRequest): void {
    this.query.set(q);
  }

  onColumnsChange(cols: TableColumn<ManagedUser>[]): void {
    this.columns.set(cols);
  }

  load(): void {
    this.loading.set(true);
    this.userService.getManagedUsers().subscribe({
      next: (list) => {
        this.allUsers.set(list ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.allUsers.set([]);
        this.loading.set(false);
        this.toast.error('Failed to load users');
        this.logger.error('Load managed users failed', err, LOG_FILE, 'load');
      },
    });
  }

  loadPendingCount(): void {
    this.userService.getPendingUsers().subscribe({
      next: (list) => this.pendingCount.set(list?.length ?? 0),
      error: () => this.pendingCount.set(0),
    });
  }

  openRoles(user: ManagedUser): void {
    if (this.isSelf(user)) {
      this.toast.warning('You cannot change your own roles');
      return;
    }
    this.selectedUser.set(user);
    this.selectedRoles.set([...(user.roles || [])]);
    this.showRoles.set(true);
  }

  closeRoles(): void {
    this.showRoles.set(false);
    this.selectedUser.set(null);
  }

  toggleRole(role: string, checked: boolean): void {
    this.selectedRoles.update((roles) =>
      checked ? (roles.includes(role) ? roles : [...roles, role]) : roles.filter((r) => r !== role),
    );
  }

  isRoleSelected(role: string): boolean {
    return this.selectedRoles().includes(role);
  }

  saveRoles(): void {
    const user = this.selectedUser();
    if (!user) return;
    const roles = this.selectedRoles();
    if (roles.length === 0) {
      this.toast.warning('Select at least one role');
      return;
    }

    this.saving.set(true);
    this.userService.assignRoles(user.identityUserId, roles).subscribe({
      next: (ok) => {
        this.saving.set(false);
        if (ok) {
          this.toast.success('Roles updated');
          this.closeRoles();
          this.load();
        } else {
          this.toast.error('Failed to update roles');
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.toast.error('Failed to update roles');
        this.logger.error('Assign roles failed', err, LOG_FILE, 'saveRoles');
      },
    });
  }

  async activate(user: ManagedUser): Promise<void> {
    if (this.isSelf(user)) {
      this.toast.warning('You cannot activate your own account');
      return;
    }
    if (this.anyBusy()) return;

    const ok = await this.confirm.open({
      title: 'Activate user',
      message: `Activate ${user.fullName}?`,
      confirmText: 'Activate',
      confirmColor: 'success',
    });
    if (!ok) return;

    this.busyId.set(user.identityUserId);
    this.userService.activateUser(user.identityUserId).subscribe({
      next: (success) => {
        this.busyId.set(null);
        if (success) {
          this.toast.success('User activated');
          this.load();
        } else {
          this.toast.error('Failed to activate');
        }
      },
      error: (err) => {
        this.busyId.set(null);
        this.toast.error('Failed to activate');
        this.logger.error('Activate failed', err, LOG_FILE, 'activate');
      },
    });
  }

  async deactivate(user: ManagedUser): Promise<void> {
    if (this.isSelf(user)) {
      this.toast.warning('You cannot deactivate your own account');
      return;
    }
    if (this.anyBusy()) return;

    const ok = await this.confirm.open({
      title: 'Deactivate user',
      message: `Deactivate ${user.fullName}?`,
      confirmText: 'Deactivate',
      confirmColor: 'danger',
    });
    if (!ok) return;

    this.busyId.set(user.identityUserId);
    this.userService.deactivateUser(user.identityUserId).subscribe({
      next: (success) => {
        this.busyId.set(null);
        if (success) {
          this.toast.success('User deactivated');
          this.load();
        } else {
          this.toast.error('Failed to deactivate');
        }
      },
      error: (err) => {
        this.busyId.set(null);
        this.toast.error('Failed to deactivate');
        this.logger.error('Deactivate failed', err, LOG_FILE, 'deactivate');
      },
    });
  }
}
