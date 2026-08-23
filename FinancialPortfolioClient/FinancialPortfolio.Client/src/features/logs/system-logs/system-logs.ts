import { Component, OnInit, inject, signal, ChangeDetectionStrategy, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { DataGrid } from '../../../layout/components/table/data-grid/data-grid';
import { TableColumn } from '../../../core/models/query/table-column.model';
import { QueryRequest } from '../../../core/models/query/query-request.model';
import { SystemLog } from '../../../core/models/system-log/system-log.model';
import { SystemLogService } from '../../../core/services/logs/system-log.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { ToastService } from '../../../core/services/toast/toast.service';
import { PageHeader } from '../../../layout/components/page-header/page-header';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';

@Component({
  selector: 'app-system-logs',
  standalone: true,
  imports: [CommonModule, DataGrid, PageHeader],
  templateUrl: './system-logs.html',
  styleUrl: './system-logs.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SystemLogs implements OnInit {
  private readonly systemLogService = inject(SystemLogService);
  private readonly logger = inject(ClientLoggerService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly logs = signal<SystemLog[]>([]);
  readonly totalRecords = signal(0);
  readonly loading = signal(false);

  readonly query = signal<QueryRequest>({
    pageNumber: 1,
    pageSize: 10,
  });

  readonly headerActions = computed<PageHeaderAction[]>(() => [
  {
    id: 'refresh',
    label: 'Refresh',
    icon: 'bi-arrow-clockwise',
    color: 'outline-secondary',
    disabled: this.loading(),
  }
]);

  readonly columns = signal<TableColumn<SystemLog>[]>([
    {
      key: 'id',
      header: 'ID',
      sortable: false,
      width: '80px',
      canToggle: false,
      hidden: true,
    },
    {
      key: 'logLevel',
      header: 'Level',
      sortable: false,
      filterable: true,
      width: '120px',
    },
    {
      key: 'applicationLevel',
      header: 'Source',
      sortable: false,
      filterable: true,
      width: '120px',
    },
    {
      key: 'category',
      header: 'Category',
      sortable: false,
      filterable: true,
    },
    {
      key: 'method',
      header: 'Method',
      sortable: true,
      filterable: true,
      width: '140px',
    },
    {
      key: 'message',
      header: 'Message',
      filterable: true,
      hidden: true,
    },
    {
      key: 'requestPath',
      header: 'Path',
      filterable: true,
      hidden: true,
    },
    {
      key: 'ipAddress',
      header: 'IP',
      hidden: true,
    },
    {
      key: 'createdDate',
      header: 'Date',
      sortable: true,
      width: '180px',
      formatter: (row) => (row.createdDate ? new Date(row.createdDate).toLocaleString() : ''),
    },
    {
      key: 'actions',
      header: 'Actions',
      type: 'actions',
      canToggle: false,
      width: '100px',
      actions: [
        {
          icon: 'bi-eye',
          label: 'View',
          color: 'outline-primary',
          click: (row) => this.viewLog(row),
        },
      ],
    },
  ]);

  ngOnInit(): void {
    this.load();
  }

  onQueryChange(query: QueryRequest): void {
    this.query.set(query);
    this.load();
  }

  onColumnsChange(cols: TableColumn<SystemLog>[]): void {
    this.columns.set(cols);
  }

  load(): void {
    this.loading.set(true);

    this.systemLogService.getAll(this.query()).subscribe({
      next: (result) => {
        this.logs.set(result?.data ?? []);
        this.totalRecords.set(result?.totalRecords ?? 0);
        this.loading.set(false);
      },
      error: (err) => {
        this.logs.set([]);
        this.totalRecords.set(0);
        this.loading.set(false);
        this.toast.error('Failed to load system logs');
        this.logger.error('Failed to load system logs', err, 'SystemLogs');
      },
    });
  }

  viewLog(row: SystemLog): void {
    void this.router.navigate(['/system-logs', row.id]);
  }

  onHeaderAction(id: string): void {
  if (id === 'refresh') this.refresh();
}

refresh(): void {
    this.load();
  }
}
