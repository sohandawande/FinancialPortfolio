import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule, CurrencyPipe, DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { PageHeader } from '../../../layout/components/page-header/page-header';
import { DataGrid } from '../../../layout/components/table/data-grid/data-grid';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';
import { TableColumn } from '../../../core/models/query/table-column.model';
import { QueryRequest } from '../../../core/models/query/query-request.model';
import { applyClientQuery } from '../../../core/utilities/query/apply-client-query';
import { PortfolioService } from '../../../core/services/portfolio/portfolio.service';
import { ToastService } from '../../../core/services/toast/toast.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { PortfolioPosition } from '../../../core/models/portfolio/portfolio-position.model';
import { PortfolioPositionFilter } from '../../../core/models/portfolio/portfolio-position-filter.model';
import { PortfolioSummary } from '../../../core/models/portfolio/portfolio-summary.model';
import { PortfolioDividendYearTotal } from '../../../core/models/portfolio/portfolio-dividend-year-total.model';
import {
  FpDropdownSelect,
  FpDropdownSelectOption,
} from '../../../layout/components/fp-dropdown-select/fp-dropdown-select';

const LOG_FILE = 'holdings-list.ts';

@Component({
  selector: 'app-holdings-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PageHeader,
    DataGrid,
    FpDropdownSelect,
    CurrencyPipe,
    DecimalPipe,
    DatePipe,
  ],
  templateUrl: './holdings-list.html',
  styleUrl: './holdings-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HoldingsList implements OnInit {
  private readonly portfolioService = inject(PortfolioService);
  private readonly toast = inject(ToastService);
  private readonly logger = inject(ClientLoggerService);
  private readonly router = inject(Router);

  readonly positions = signal<PortfolioPosition[]>([]);
  readonly summary = signal<PortfolioSummary | null>(null);
  readonly loading = signal(false);
  readonly statusFilter = signal<PortfolioPositionFilter>('all');
  readonly selectedDividendYear = signal<number>(new Date().getFullYear());
  readonly query = signal<QueryRequest>({ pageNumber: 1, pageSize: 10 });
  readonly searchKeys = ['symbol', 'companyName', 'stockCode'];

  readonly filtered = computed(() =>
    applyClientQuery(this.positions(), { ...this.query(), pageNumber: 1, pageSize: this.positions().length || 1 }, this.searchKeys)
      .page,
  );

  readonly holdingCount = computed(() => this.positions().filter((p) => p.status === 1).length);
  readonly soldCount = computed(() => this.positions().filter((p) => p.status === 2).length);

  readonly dividendsByYear = computed<PortfolioDividendYearTotal[]>(() => {
    return [...(this.summary()?.dividendsByYear ?? [])].sort((a, b) => b.year - a.year);
  });

  readonly selectedYearTotal = computed<PortfolioDividendYearTotal>(() => {
    const year = this.selectedDividendYear();
    return (
      this.dividendsByYear().find((y) => y.year === year) ?? {
        year,
        amount: 0,
        count: 0,
      }
    );
  });

  readonly yearSharePercent = computed(() => {
    const lifetime = this.summary()?.totalDividendsReceived ?? 0;
    if (lifetime <= 0) return 0;
    return Number(((this.selectedYearTotal().amount / lifetime) * 100).toFixed(1));
  });

  readonly headerActions = computed<PageHeaderAction[]>(() => [
    {
      id: 'refresh',
      label: 'Refresh',
      icon: 'bi-arrow-clockwise',
      color: 'outline-secondary',
      disabled: this.loading(),
    },
    {
      id: 'dividends',
      label: 'Dividends',
      icon: 'bi-cash-coin',
      color: 'outline-primary',
    },
    {
      id: 'export',
      label: 'Export',
      icon: 'bi-download',
      color: 'outline-primary',
      disabled: this.loading() || this.filtered().length === 0,
    },
  ]);

  readonly yearOptions = computed<FpDropdownSelectOption[]>(() => {
    const years = this.dividendsByYear();
    if (years.length === 0) {
      return [{ value: this.selectedDividendYear(), label: 'No years yet' }];
    }
    return years.map((y) => ({ value: y.year, label: String(y.year) }));
  });

  onYearChange(year: number | string | null): void {
    if (year === null || year === '') return;
    this.selectedDividendYear.set(Number(year));
  }

  ngOnInit(): void {
    this.load();
  }

  onHeaderAction(actionId: string): void {
    if (actionId === 'refresh') this.load();
    if (actionId === 'dividends') void this.router.navigate(['/dividends']);
    if (actionId === 'export') this.exportCsv();
  }

  readonly columns = signal<TableColumn<PortfolioPosition>[]>([
    { key: 'companyName', header: 'Company', type: 'stock', sortable: true },
    {
      key: 'status',
      header: 'Status',
      type: 'badge',
      sortable: true,
      formatter: (row) => this.statusBadge(row.status).text,
      badgeClass: (row) => this.statusBadge(row.status).class,
    },
    { key: 'currentQuantity', header: 'Net Qty', type: 'number', sortable: true, align: 'end' },
    { key: 'averageBuyPrice', header: 'Avg Buy', type: 'money', sortable: true },
    { key: 'marketPrice', header: 'Market', type: 'money', sortable: true },
    { key: 'totalInvestment', header: 'Investment', type: 'money', sortable: true },
    { key: 'totalCurrentValue', header: 'Current', type: 'money', sortable: true },
    { key: 'unrealizedGainLoss', header: 'Unrealized', type: 'pnl', sortable: true },
    { key: 'realizedGainLoss', header: 'Realized', type: 'pnl', sortable: true, hidden: true },
    { key: 'totalDividends', header: 'Dividend', type: 'money', sortable: true, hidden: true },
    {
      key: 'totalGainLoss',
      header: 'Total P&L',
      type: 'pnl',
      sortable: true,
      percentKey: 'gainLossPercent',
    },
    { key: 'holdDays', header: 'Days', type: 'number', sortable: true, align: 'center', hidden: true },
    {
      key: 'buyLotCount',
      header: 'Lots',
      type: 'badge',
      sortable: true,
      hidden: true,
      formatter: (row) => `${row.buyLotCount} buy`,
      badgeClass: () => 'text-bg-light border',
    },
    {
      key: 'actions',
      header: '',
      type: 'actions',
      canToggle: false,
      width: '88px',
      actions: [
        {
          icon: 'bi-eye',
          label: 'View',
          color: 'outline-primary',
          click: (row) => this.openDetail(row),
        },
        {
          icon: 'bi-cash-coin',
          label: 'Dividends',
          color: 'outline-success',
          click: (row) => this.openDividends(row),
        },
      ],
    },
  ]);

  setStatus(filter: PortfolioPositionFilter): void {
    this.statusFilter.set(filter);
    this.query.update((q) => ({ ...q, pageNumber: 1 }));
    this.load();
  }

  onQueryChange(next: QueryRequest): void {
    this.query.set(next);
  }

  onColumnsChange(cols: TableColumn<PortfolioPosition>[]): void {
    this.columns.set(cols);
  }

  load(): void {
    this.loading.set(true);
    this.portfolioService.getSummary().subscribe({
      next: (s) => {
        this.summary.set(s);
        this.syncSelectedYear(s?.dividendsByYear ?? []);
      },
      error: (err) => this.logger.error('Failed to load summary', err, LOG_FILE),
    });
    this.portfolioService.getPositions(this.statusFilter()).subscribe({
      next: (rows) => {
        this.positions.set(rows);
        this.loading.set(false);
      },
      error: (err) => {
        this.logger.error('Failed to load positions', err, LOG_FILE);
        this.loading.set(false);
        this.toast.error('Failed to load holdings');
      },
    });
  }

  openDetail(row: PortfolioPosition): void {
    void this.router.navigate(['/holdings', row.stockId]);
  }

  openDividends(row?: PortfolioPosition, event?: Event): void {
    event?.stopPropagation();
    if (row) {
      void this.router.navigate(['/dividends'], { queryParams: { stockId: row.stockId } });
      return;
    }
    void this.router.navigate(['/dividends']);
  }

  formatPnL(value: number): string {
    const sign = value >= 0 ? '+' : '';
    return `${sign}${value.toFixed(2)}`;
  }

  statusBadge(status: number): { text: string; class: string } {
    return status === 2
      ? { text: 'Fully Sold', class: 'bg-secondary-subtle text-secondary' }
      : { text: 'Holding', class: 'bg-success-subtle text-success' };
  }

  private syncSelectedYear(years: PortfolioDividendYearTotal[]): void {
    if (years.length === 0) return;
    const current = this.selectedDividendYear();
    if (years.some((y) => y.year === current)) return;
    this.selectedDividendYear.set([...years].sort((a, b) => b.year - a.year)[0].year);
  }

  private exportCsv(): void {
    const data = this.filtered();
    if (data.length === 0) {
      this.toast.warning('No rows to export');
      return;
    }
    const headers = [
      'S.No',
      'Company',
      'Symbol',
      'Status',
      'Net Qty',
      'Avg Buy',
      'Market',
      'Investment',
      'Current Value',
      'Unrealized',
      'Realized',
      'Dividends',
      'Total P&L',
      '%',
      'Days',
      'First Buy',
      'Last Exit',
    ];
    const rows = data.map((p) => [
      p.serialNo,
      p.companyName,
      p.symbol,
      p.status === 2 ? 'Fully Sold' : 'Holding',
      p.currentQuantity,
      p.averageBuyPrice,
      p.marketPrice,
      p.totalInvestment,
      p.totalCurrentValue,
      p.unrealizedGainLoss,
      p.realizedGainLoss,
      p.totalDividends,
      p.totalGainLoss,
      p.gainLossPercent,
      p.holdDays,
      p.firstPurchaseDate,
      p.lastExitDate ?? '',
    ]);
    const csv = [
      headers.join(','),
      ...rows.map((row) =>
        row
          .map((cell) => {
            const value = cell?.toString() ?? '';
            return value.includes(',') || value.includes('"')
              ? `"${value.replace(/"/g, '""')}"`
              : value;
          })
          .join(','),
      ),
    ].join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `holdings-${this.statusFilter()}.csv`;
    link.click();
    URL.revokeObjectURL(url);
    this.toast.success('Holdings exported');
  }
}
