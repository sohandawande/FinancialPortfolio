import {
  Component,
  OnInit,
  inject,
  signal,
  ChangeDetectionStrategy,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { DataGrid } from '../../../layout/components/table/data-grid/data-grid';
import { EtfService } from '../../../core/services/etf/etf.service';
import { GoogleSheetService } from '../../../core/services/google-sheet/google-sheet.service';
import { GoogleSheetSyncResponse } from '../../../core/models/google-sheet/google-sheet-sync.model';
import { ToastService } from '../../../core/services/toast/toast.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { ConfirmModalService } from '../../../core/services/confirm-modal/confirm-modal.service';
import { Etf } from '../../../core/models/etf/etf.model';
import { QueryRequest } from '../../../core/models/query/query-request.model';
import { TableColumn } from '../../../core/models/query/table-column.model';
import { PageHeader } from '../../../layout/components/page-header/page-header';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';
import {
  formatStockSymbolWithLogoHtml,
  formatStockCompanyHtml,
  formatStockPriceHtml,
  formatStockChangeHtml,
  formatStockActiveHtml,
  formatStockCategoryHtml,
  formatStockMoney,
  formatStockNumber,
} from '../../../core/helper/formatters/stock-format.helper';

const LOG_FILE = 'etf-lists.ts';

@Component({
  selector: 'app-etf-lists',
  standalone: true,
  imports: [CommonModule, DataGrid, PageHeader],
  templateUrl: './etf-lists.html',
  styleUrl: './etf-lists.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EtfLists implements OnInit {
  private readonly etfService = inject(EtfService);
  private readonly googleSheetService = inject(GoogleSheetService);
  private readonly toast = inject(ToastService);
  private readonly logger = inject(ClientLoggerService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly router = inject(Router);

  readonly etfs = signal<Etf[]>([]);
  readonly loading = signal(false);
  readonly syncing = signal(false);
  readonly totalRecords = signal(0);

  readonly query = signal<QueryRequest>({
    pageNumber: 1,
    pageSize: 10,
    globalSearch: '',
    filters: [],
    sorts: [],
  });

  readonly headerActions = computed<PageHeaderAction[]>(() => [
    {
      id: 'refresh',
      label: 'Refresh',
      icon: 'bi-arrow-clockwise',
      color: 'outline-secondary',
      disabled: this.loading() || this.syncing(),
    },
    {
      id: 'force-refresh',
      label: this.syncing() ? 'Syncing…' : 'Force Refresh',
      icon: 'bi-cloud-download',
      color: 'primary',
      loading: this.syncing(),
      disabled: this.loading() || this.syncing(),
    },
  ]);

    readonly columns = signal<TableColumn<Etf>[]>([
    {
      key: 'symbol',
      header: 'Symbol',
      sortable: true,
      filterable: true,
      type: 'text',
      canToggle: false,
      formatter: (row) => formatStockSymbolWithLogoHtml(row.symbol, row.logoUrl),
    },
    {
      key: 'companyName',
      header: 'Company',
      sortable: true,
      filterable: true,
      type: 'text',
      canToggle: true,
      formatter: (row) => formatStockCompanyHtml(row.companyName),
    },
    {
      key: 'industry',
      header: 'Industry',
      sortable: true,
      filterable: true,
      type: 'text',
      canToggle: true,
    },
    {
      key: 'isinCode',
      header: 'ISIN',
      sortable: true,
      filterable: true,
      type: 'text',
      canToggle: true,
      hidden: true,
    },
    {
      key: 'category',
      header: 'Category',
      sortable: true,
      filterable: true,
      type: 'badge',
      canToggle: true,
      formatter: (row) => formatStockCategoryHtml(row.category),
    },
    {
      key: 'currentPrice',
      header: 'Price',
      sortable: true,
      filterable: true,
      type: 'number',
      canToggle: true,
      formatter: (row) => formatStockPriceHtml(row.currentPrice),
    },
    {
      key: 'priceChange',
      header: 'Change',
      sortable: true,
      type: 'number',
      canToggle: true,
      formatter: (row) => formatStockChangeHtml(row.priceChange),
    },
    {
      key: 'previousClose',
      header: 'Close',
      sortable: true,
      type: 'number',
      canToggle: true,
      hidden: true,
      formatter: (row) => formatStockMoney(row.previousClose),
    },
    {
      key: 'openPrice',
      header: 'Open',
      sortable: true,
      type: 'number',
      canToggle: true,
      hidden: true,
      formatter: (row) => formatStockMoney(row.openPrice),
    },
    {
      key: 'highPrice',
      header: 'High',
      sortable: true,
      type: 'number',
      canToggle: true,
      hidden: true,
      formatter: (row) => formatStockMoney(row.highPrice),
    },
    {
      key: 'lowPrice',
      header: 'Low',
      sortable: true,
      type: 'number',
      canToggle: true,
      hidden: true,
      formatter: (row) => formatStockMoney(row.lowPrice),
    },
    {
      key: 'volume',
      header: 'Volume',
      sortable: true,
      type: 'number',
      canToggle: true,
      hidden: true,
      formatter: (row) => formatStockNumber(row.volume),
    },
    {
      key: 'week52High',
      header: '52W High',
      canToggle: true,
      hidden: true,
      formatter: (row) => formatStockMoney(row.week52High),
    },
    {
      key: 'week52Low',
      header: '52W Low',
      canToggle: true,
      hidden: true,
      formatter: (row) => formatStockMoney(row.week52Low),
    },
    {
      key: 'pe',
      header: 'P/E',
      sortable: true,
      type: 'number',
      canToggle: true,
      hidden: true,
      formatter: (row) => (row.pe ? row.pe.toFixed(2) : '—'),
    },
    {
      key: 'eps',
      header: 'EPS',
      sortable: true,
      type: 'number',
      canToggle: true,
      hidden: true,
      formatter: (row) => (row.eps ? row.eps.toFixed(2) : '—'),
    },
    {
      key: 'marketCap',
      header: 'Market Cap',
      sortable: true,
      type: 'number',
      canToggle: true,
      hidden: true,
      formatter: (row) => formatStockMoney(row.marketCap),
    },
    {
      key: 'isActive',
      header: 'Active',
      type: 'boolean',
      canToggle: false,
      formatter: (row) => formatStockActiveHtml(row.isActive),
    },
    {
      key: 'lastUpdated',
      header: 'Updated',
      sortable: true,
      type: 'date',
      canToggle: true,
      hidden: true,
      formatter: (row) => (row.lastUpdated ? new Date(row.lastUpdated).toLocaleString() : ''),
    },
    {
      key: 'actions',
      header: 'Actions',
      type: 'actions',
      canToggle: false,
      actions: [
        {
          icon: 'bi-eye',
          label: 'View',
          color: 'outline-primary',
          click: (row) => this.viewEtf(row),
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

  onColumnsChange(cols: TableColumn<Etf>[]): void {
    this.columns.set(cols);
  }

  refresh(): void {
    this.load();
  }

  async forceRefresh(): Promise<void> {
    const ok = await this.confirm.open({
      title: 'Force Refresh ETFs',
      message: 'Sync market data from Google Sheet? (Uses stock sheet sync until a dedicated ETF sheet exists.)',
      confirmText: 'Sync Now',
      confirmColor: 'primary',
    });

    if (!ok) return;

    this.syncing.set(true);

    this.googleSheetService.syncStocks(true).subscribe({
      next: (result: GoogleSheetSyncResponse | null) => {
        this.syncing.set(false);

        if (!result) {
          this.toast.error('Sync failed');
          return;
        }

        this.toast.success(
          `Synced: ${result.totalRecords} total | ` +
            `+${result.insertedRecords} new | ` +
            `↻${result.updatedRecords} updated | ` +
            `⊘${result.skippedRecords} skipped`,
        );

        this.logger.info(`Google Sheet sync: ${JSON.stringify(result)}`, LOG_FILE, 'forceRefresh');

        this.load();
      },
      error: (err: unknown) => {
        this.syncing.set(false);
        this.toast.error('Failed to sync from Google Sheet');
        this.logger.error('Google Sheet sync failed', err, LOG_FILE, 'forceRefresh');
      },
    });
  }

  load(): void {
    this.loading.set(true);

    this.etfService.search(this.query()).subscribe({
      next: (result) => {
        this.etfs.set(result?.data ?? []);
        this.totalRecords.set(result?.totalRecords ?? 0);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.etfs.set([]);
        this.totalRecords.set(0);
        this.loading.set(false);
        this.toast.error('Failed to load etfs');
        this.logger.error('Failed to load etfs', err, LOG_FILE, 'load');
      },
    });
  }

  viewEtf(row: Etf): void {
    void this.router.navigate(['/etfs', row.id]);
  }

  onHeaderAction(id: string): void {
    if (id === 'refresh') this.refresh();
    if (id === 'force-refresh') void this.forceRefresh();
  }
}
