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
import { PendingUser } from '../../../core/models/user/pending-user.model';
import { UserService } from '../../../core/services/user/user.service';
import { ToastService } from '../../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../../core/services/confirm-modal/confirm-modal.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { ASSIGNABLE_ROLES } from '../../../core/constants/user/role.constants';
import { QueryRequest } from '../../../core/models/query/query-request.model';
import { TableColumn } from '../../../core/models/query/table-column.model';
import { SortDirection } from '../../../core/models/query/sort-direction.enum';

const LOG_FILE = 'pending-users.ts';

@Component({
  selector: 'app-pending-users',
  standalone: true,
  imports: [CommonModule, DataGrid, PageHeader, FpModal],
  templateUrl: './pending-users.html',
  styleUrl: './pending-users.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PendingUsers implements OnInit {
  private readonly userService = inject(UserService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly logger = inject(ClientLoggerService);
  private readonly router = inject(Router);
  private readonly realtime = inject(RealtimeService);
  private pendingSub?: Subscription;

  readonly allUsers = signal<PendingUser[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly showApprove = signal(false);
  readonly selectedUser = signal<PendingUser | null>(null);
  readonly selectedRoles = signal<string[]>([]);
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
      id: 'manage',
      label: 'Manage users',
      icon: 'bi-people-fill',
      color: 'outline-secondary',
    },
    {
      id: 'refresh',
      label: 'Refresh',
      icon: 'bi-arrow-clockwise',
      color: 'primary',
      disabled: this.loading(),
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
          u.userCode.toLowerCase().includes(q)
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

  readonly columns = signal<TableColumn<PendingUser>[]>([
    {
      key: 'userCode',
      header: 'Code',
      sortable: true,
      filterable: true,
      width: '120px',
    },
    {
      key: 'fullName',
      header: 'Name',
      sortable: true,
      filterable: true,
    },
    {
      key: 'email',
      header: 'Email',
      sortable: true,
      filterable: true,
    },
    {
      key: 'createdDate',
      header: 'Registered',
      sortable: true,
      width: '160px',
      formatter: (row) =>
        row.createdDate ? new Date(row.createdDate).toLocaleString() : '—',
    },
    {
      key: 'actions',
      header: 'Actions',
      type: 'actions',
      canToggle: false,
      width: '100px',
      actions: [
        {
          icon: 'bi-check2-circle',
          label: 'Approve',
          color: 'outline-success',
          click: (row) => this.openApprove(row),
        },
      ],
    },
  ]);

  ngOnInit(): void {
    this.load();
    this.pendingSub = this.realtime.onPendingUserCreated.subscribe((user) => {
      this.load();
    });
  }

  ngOnDestroy(): void {
    this.pendingSub?.unsubscribe();
  }

  onHeaderAction(id: string): void {
    if (id === 'refresh') this.load();
    if (id === 'manage') void this.router.navigate(['/users/manage']);
  }

  onQueryChange(q: QueryRequest): void {
    this.query.set(q);
  }

  onColumnsChange(cols: TableColumn<PendingUser>[]): void {
    this.columns.set(cols);
  }

  load(): void {
    this.loading.set(true);
    this.userService.getPendingUsers().subscribe({
      next: (list) => {
        this.allUsers.set(list ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.allUsers.set([]);
        this.loading.set(false);
        this.toast.error('Failed to load pending users');
        this.logger.error('Load pending users failed', err, LOG_FILE, 'load');
      },
    });
  }

  openApprove(user: PendingUser): void {
    this.selectedUser.set(user);
    this.selectedRoles.set(['User']);
    this.showApprove.set(true);
  }

  closeApprove(): void {
    this.showApprove.set(false);
    this.selectedUser.set(null);
  }

  toggleRole(role: string, checked: boolean): void {
    this.selectedRoles.update((roles) =>
      checked
        ? roles.includes(role)
          ? roles
          : [...roles, role]
        : roles.filter((r) => r !== role)
    );
  }

  isRoleSelected(role: string): boolean {
    return this.selectedRoles().includes(role);
  }

  async submitApprove(): Promise<void> {
    const user = this.selectedUser();
    const roles = this.selectedRoles();
    if (!user) return;

    if (roles.length === 0) {
      this.toast.warning('Select at least one role');
      return;
    }

    const ok = await this.confirm.open({
      title: 'Approve user',
      message: `Approve ${user.fullName} (${user.email}) with roles: ${roles.join(', ')}?`,
      confirmText: 'Approve',
      confirmColor: 'success',
    });
    if (!ok) return;

    this.saving.set(true);
    this.userService.approveUser(user.identityUserId, roles).subscribe({
      next: (success) => {
        this.saving.set(false);
        if (success) {
          this.toast.success('User approved and activated');
          this.closeApprove();
          this.load();
        } else {
          this.toast.error('Failed to approve user');
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.toast.error('Failed to approve user');
        this.logger.error('Approve user failed', err, LOG_FILE, 'submitApprove');
      },
    });
  }
}