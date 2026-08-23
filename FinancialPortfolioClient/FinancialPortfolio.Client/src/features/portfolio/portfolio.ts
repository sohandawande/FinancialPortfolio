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
import { firstValueFrom } from 'rxjs';

import { PortfolioService } from '../../core/services/portfolio/portfolio.service';
import { StockService } from '../../core/services/stock/stock.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../core/services/confirm-modal/confirm-modal.service';
import { ClientLoggerService } from '../../core/services/logs/client-logger.service';
import { PageHeader } from '../../layout/components/page-header/page-header';
import { StockLogo } from '../../layout/components/stock-logo/stock-logo';
import { FpModal } from '../../layout/components/fp-modal/fp-modal';
import { DataGrid } from '../../layout/components/table/data-grid/data-grid';
import { PageHeaderAction } from '../../core/models/page-header/page-header-action.model';
import { TableColumn } from '../../core/models/query/table-column.model';
import { Portfolio as PortfolioModel } from '../../core/models/portfolio/portfolio.model';
import { PortfolioSummary } from '../../core/models/portfolio/portfolio-summary.model';
import { PortfolioHolding } from '../../core/models/portfolio/portfolio-holding.model';
import { PortfolioSold } from '../../core/models/portfolio/portfolio-sold.model';
import { PortfolioLedgerItem } from '../../core/models/portfolio/portfolio-ledger-item.model';
import { PortfolioLedgerFilter } from '../../core/models/portfolio/portfolio-ledger-filter.model';
import { BuyStockRequest } from '../../core/models/portfolio/buy-stock-request.model';
import { UpdateHoldRequest } from '../../core/models/portfolio/update-hold-request.model';
import { SellStockRequest } from '../../core/models/portfolio/sell-stock-request.model';
import { UpdateSoldRequest } from '../../core/models/portfolio/update-sold-request.model';
import { CreatePortfolioRequest } from '../../core/models/portfolio/create-portfolio-request.model';
import { UpdatePortfolioRequest } from '../../core/models/portfolio/update-portfolio-request.model';
import { Stock } from '../../core/models/stock/stock.model';
import { QueryRequest } from '../../core/models/query/query-request.model';
import { FpDropdownSelect, FpDropdownSelectOption } from '../../layout/components/fp-dropdown-select/fp-dropdown-select';
import { FpDate } from '../../layout/components/fp-date/fp-date';
import { PORTFOLIO_LEDGER_FILTERS } from '../../core/constants/portfolio/portfolio-ledger-filter.constants';
import {
  CsvImportKind,
  CsvImportRow,
  downloadTextFile,
  parseImportCsv,
  sampleCsv,
} from '../../core/utils/csv.util';
import { isFutureIsoDate } from '../../core/helper/validators/app.validators';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';

const LOG_FILE = 'portfolio.ts';

@Component({
  selector: 'app-portfolio',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeader, StockLogo, FpModal, DataGrid, CurrencyPipe, DecimalPipe, DatePipe, FpDropdownSelect, FpDate],
  templateUrl: './portfolio.html',
  styleUrls: ['./portfolio.css', '../../ui-styles/components/import-modal.css', '../../ui-styles/components/stock-search.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Portfolio implements OnInit {
  private readonly portfolioService = inject(PortfolioService);
  private readonly stockService = inject(StockService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly logger = inject(ClientLoggerService);
  private readonly router = inject(Router);

  readonly portfolio = signal<PortfolioModel | null>(null);
  readonly summary = signal<PortfolioSummary | null>(null);
  readonly holdings = signal<PortfolioHolding[]>([]);
  readonly soldHistory = signal<PortfolioSold[]>([]);
  readonly ledger = signal<PortfolioLedgerItem[]>([]);
  readonly loading = signal(false);
  readonly soldLoading = signal(false);
  readonly ledgerLoading = signal(false);
  readonly searchTerm = signal('');
  readonly activeTab = signal<'holdings' | 'sold' | 'ledger'>('holdings');
  readonly ledgerFilter = signal<PortfolioLedgerFilter>('lifetime');
  readonly ledgerFilters = PORTFOLIO_LEDGER_FILTERS.filter((f) => f !== 'all');
  readonly hasPortfolio = computed(() => !!this.portfolio()?.id);

  readonly filterStatus = signal<number | null>(null);
  readonly filterExchange = signal<number | null>(null);
  readonly filterFromDate = signal<string>('');
  readonly filterToDate = signal<string>('');

  readonly showBuyModal = signal(false);
  readonly buySaving = signal(false);
  readonly buyAttempted = signal(false);
  readonly buyTouched = signal({ stock: false, quantity: false, price: false, date: false });
  readonly editingHoldId = signal<number | null>(null);
  readonly holdSoldQuantity = signal(0);
  readonly isEditingHold = computed(() => this.editingHoldId() !== null);
  readonly stockSearch = signal('');
  readonly stockOptions = signal<Stock[]>([]);
  readonly stockSearching = signal(false);

  buyForm = {
    stockId: 0,
    symbol: '',
    companyName: '',
    logoUrl: null as string | null,
    quantity: 1,
    purchasePrice: 0,
    purchaseDate: new Date().toISOString().substring(0, 10),
    exchange: 1,
    notes: '',
  };

  readonly showSellModal = signal(false);
  readonly sellSaving = signal(false);
  readonly sellAttempted = signal(false);
  readonly sellTouched = signal({ quantity: false, price: false, date: false });
  readonly selectedHold = signal<PortfolioHolding | null>(null);
  readonly sellAvailableQty = signal(0);

  readonly showEditSoldModal = signal(false);
  readonly editSoldSaving = signal(false);
  readonly editSoldAttempted = signal(false);
  readonly editSoldTouched = signal({ quantity: false, price: false, date: false });
  readonly selectedSold = signal<PortfolioSold | null>(null);
  readonly editSoldMaxQty = signal(0);

  editSoldForm = {
    sellQuantity: 1,
    sellPrice: 0,
    soldDate: new Date().toISOString().substring(0, 10),
    notes: '',
  };

  sellForm = {
    sellQuantity: 1,
    sellPrice: 0,
    soldDate: new Date().toISOString().substring(0, 10),
    notes: '',
  };

  readonly showPortfolioModal = signal(false);
  readonly portfolioSaving = signal(false);
  readonly portfolioAttempted = signal(false);
  readonly portfolioNameTouched = signal(false);
  readonly isEditPortfolio = signal(false);
  portfolioForm = {
    name: 'My Portfolio',
    description: '',
  };

  readonly showImportModal = signal(false);
  readonly importSaving = signal(false);
  readonly importKind = signal<CsvImportKind>('buy');
  readonly importPreview = signal<CsvImportRow[]>([]);
  readonly importErrors = signal<string[]>([]);
  readonly importFileName = signal('');

  touchBuy(field: 'stock' | 'quantity' | 'price' | 'date'): void {
    this.buyTouched.update((t) => ({ ...t, [field]: true }));
  }

  buyFieldError(field: 'stock' | 'quantity' | 'price' | 'date'): string {
    if (!(this.buyAttempted() || this.buyTouched()[field])) return '';
    if (field === 'stock' && !this.buyForm.stockId) return 'Select a stock';
    if (field === 'quantity') {
      const min = this.isEditingHold() ? Math.max(1, this.holdSoldQuantity()) : 1;
      if (!this.buyForm.quantity || this.buyForm.quantity < min) {
        return this.isEditingHold() && this.holdSoldQuantity() > 0
          ? `Quantity cannot be below ${this.holdSoldQuantity()} already sold from this lot`
          : 'Quantity must be at least 1';
      }
    }
    if (field === 'price' && (!(this.buyForm.purchasePrice > 0))) return 'Enter a purchase price greater than 0';
    if (field === 'date') {
      if (!this.buyForm.purchaseDate) return 'Purchase date is required';
      if (isFutureIsoDate(this.buyForm.purchaseDate)) return 'Purchase date cannot be in the future';
    }
    return '';
  }

  touchSell(field: 'quantity' | 'price' | 'date'): void {
    this.sellTouched.update((t) => ({ ...t, [field]: true }));
  }

  sellFieldError(field: 'quantity' | 'price' | 'date'): string {
    if (!(this.sellAttempted() || this.sellTouched()[field])) return '';
    const max = Math.max(1, this.sellAvailableQty());
    if (field === 'quantity') {
      if (this.sellForm.sellQuantity < 1) return 'Sell quantity must be at least 1';
      if (this.sellForm.sellQuantity > max) return `You can sell at most ${max} shares`;
    }
    if (field === 'price' && (!(this.sellForm.sellPrice > 0))) return 'Enter a sell price greater than 0';
    if (field === 'date') {
      if (!this.sellForm.soldDate) return 'Sold date is required';
      if (isFutureIsoDate(this.sellForm.soldDate)) return 'Sold date cannot be in the future';
    }
    return '';
  }

  touchEditSold(field: 'quantity' | 'price' | 'date'): void {
    this.editSoldTouched.update((t) => ({ ...t, [field]: true }));
  }

  editSoldFieldError(field: 'quantity' | 'price' | 'date'): string {
    if (!(this.editSoldAttempted() || this.editSoldTouched()[field])) return '';
    const max = Math.max(1, this.editSoldMaxQty());
    if (field === 'quantity') {
      if (this.editSoldForm.sellQuantity < 1) return 'Sell quantity must be at least 1';
      if (this.editSoldForm.sellQuantity > max) return `Quantity cannot exceed ${max} for this lot`;
    }
    if (field === 'price' && (!(this.editSoldForm.sellPrice > 0))) return 'Enter a sell price greater than 0';
    if (field === 'date') {
      if (!this.editSoldForm.soldDate) return 'Sold date is required';
      if (isFutureIsoDate(this.editSoldForm.soldDate)) return 'Sold date cannot be in the future';
    }
    return '';
  }

  portfolioNameError(): string {
    if (!(this.portfolioAttempted() || this.portfolioNameTouched())) return '';
    const name = this.portfolioForm.name?.trim() ?? '';
    if (!name) return 'Portfolio name is required';
    if (name.length > 100) return 'Portfolio name cannot exceed 100 characters';
    return '';
  }

  readonly holdingsQuery = signal<QueryRequest>({ pageNumber: 1, pageSize: 10 });
  readonly soldQuery = signal<QueryRequest>({ pageNumber: 1, pageSize: 10 });
  readonly ledgerQuery = signal<QueryRequest>({ pageNumber: 1, pageSize: 10 });
  readonly searchKeys = ['symbol', 'companyName', 'stockCode'];

  readonly filteredHoldings = computed(() => {
    let list = this.holdings();
    const status = this.filterStatus();
    if (status !== null) list = list.filter((h) => h.lotStatus === status);
    const exchange = this.filterExchange();
    if (exchange !== null) list = list.filter((h) => h.exchange === exchange);
    return list;
  });

  readonly filteredLedger = computed(() => {
    let list = this.ledger();
    const exchange = this.filterExchange();
    if (exchange !== null) list = list.filter((r) => r.exchange === exchange);
    return list;
  });

  readonly filteredSoldHistory = computed(() => {
    let list = this.soldHistory();
    const from = this.filterFromDate();
    const to = this.filterToDate();
    if (from) list = list.filter((s) => s.soldDate >= from);
    if (to) list = list.filter((s) => s.soldDate <= to);
    const exchange = this.filterExchange();
    if (exchange !== null) list = list.filter((s) => s.exchange === exchange);
    return list;
  });

  readonly headerActions = computed<PageHeaderAction[]>(() => {
    if (!this.hasPortfolio()) {
      return [
        {
          id: 'create',
          label: 'Create Portfolio',
          icon: 'bi-plus-lg',
          color: 'primary',
          disabled: this.loading(),
        },
      ];
    }

    return [
      { id: 'refresh', label: 'Refresh', icon: 'bi-arrow-clockwise', color: 'outline-secondary', disabled: this.loading() },
      { id: 'edit', label: 'Edit', icon: 'bi-pencil', color: 'outline-secondary', disabled: this.loading() },
      { id: 'import', label: 'Import', icon: 'bi-upload', color: 'outline-success', disabled: this.loading() },
      { id: 'export', label: 'Export', icon: 'bi-download', color: 'outline-primary', disabled: this.loading() },
      { id: 'buy', label: 'Buy Stock', icon: 'bi-plus-lg', color: 'primary', disabled: this.loading() },
    ];
  });

  readonly statusOptions: FpDropdownSelectOption[] = [
  { value: null, label: 'All status' },
  { value: 1, label: 'Open' },
  { value: 2, label: 'Partial' },
];

readonly exchangeOptions: FpDropdownSelectOption[] = [
  { value: null, label: 'All exchange' },
  { value: 1, label: 'NSE' },
  { value: 2, label: 'BSE' },
];

  readonly holdingColumns = signal<TableColumn<PortfolioHolding>[]>([
    { key: 'companyName', header: 'Company', type: 'stock', sortable: true },
    { key: 'remainingQuantity', header: 'Qty', type: 'number', sortable: true, align: 'end' },
    { key: 'purchasePrice', header: 'Avg Buy', type: 'money', sortable: true },
    { key: 'currentPrice', header: 'Market', type: 'money', sortable: true },
    { key: 'remainingInvestment', header: 'Investment', type: 'money', sortable: true },
    { key: 'currentValue', header: 'Current Value', type: 'money', sortable: true },
    { key: 'unrealizedGainLoss', header: 'P&L', type: 'pnl', sortable: true, percentKey: 'unrealizedGainLossPercent' },
    { key: 'holdDays', header: 'Days', type: 'number', sortable: true, align: 'center' },
    {
      key: 'lotStatus',
      header: 'Status',
      type: 'badge',
      sortable: true,
      formatter: (row) => this.getLotStatusBadge(row.lotStatus).text,
      badgeClass: (row) => this.getLotStatusBadge(row.lotStatus).class,
    },
    {
      key: 'actions',
      header: '',
      type: 'actions',
      canToggle: false,
      width: '128px',
      actions: [
        {
          icon: 'bi-box-arrow-right',
          label: 'Sell',
          color: 'outline-danger',
          visible: (row) => row.remainingQuantity > 0,
          click: (row) => this.openSellModal(row),
        },
        {
          icon: 'bi-pencil',
          label: 'Edit lot',
          color: 'outline-primary',
          click: (row) => this.openEditHold(row),
        },
        {
          icon: 'bi-trash',
          label: 'Delete lot',
          color: 'outline-danger',
          click: (row) => { void this.deleteHold(row); },
        },
      ],
    },
  ]);

  readonly soldColumns = signal<TableColumn<PortfolioSold>[]>([
    { key: 'companyName', header: 'Company', type: 'stock', sortable: true },
    { key: 'sellQuantity', header: 'Qty', type: 'number', sortable: true, align: 'end' },
    { key: 'purchasePrice', header: 'Buy Price', type: 'money', sortable: true },
    { key: 'sellPrice', header: 'Sell Price', type: 'money', sortable: true },
    { key: 'costAmount', header: 'Cost', type: 'money', sortable: true },
    { key: 'sellAmount', header: 'Sell Amount', type: 'money', sortable: true },
    { key: 'realizedGainLoss', header: 'P&L', type: 'pnl', sortable: true, percentKey: 'realizedGainLossPercent' },
    { key: 'holdDays', header: 'Days Held', type: 'number', sortable: true, align: 'center' },
    { key: 'soldDate', header: 'Sold Date', type: 'date', sortable: true },
    {
      key: 'actions',
      header: '',
      type: 'actions',
      canToggle: false,
      width: '96px',
      actions: [
        {
          icon: 'bi-pencil',
          label: 'Edit sell',
          color: 'outline-primary',
          click: (row) => this.openEditSold(row),
        },
        {
          icon: 'bi-trash',
          label: 'Delete sell',
          color: 'outline-danger',
          click: (row) => { void this.deleteSold(row); },
        },
      ],
    },
  ]);

  readonly ledgerColumns = signal<TableColumn<PortfolioLedgerItem>[]>([
    { key: 'companyName', header: 'Company', type: 'stock', sortable: true },
    {
      key: 'currentType',
      header: 'Type',
      type: 'badge',
      sortable: true,
      formatter: (row) => this.getActionBadge(row.currentType).text,
      badgeClass: (row) => this.getActionBadge(row.currentType).class,
    },
    { key: 'netQuantity', header: 'Qty', type: 'number', sortable: true, align: 'end' },
    { key: 'purchasePrice', header: 'Buy', type: 'money', sortable: true },
    {
      key: 'marketPrice',
      header: 'Market / Sell',
      type: 'money',
      sortable: true,
      formatter: (row) => row.currentType === 2 ? (row.sellPrice ?? row.marketPrice) : row.marketPrice,
    },
    { key: 'totalInvestment', header: 'Investment', type: 'money', sortable: true },
    {
      key: 'totalCurrentValue',
      header: 'Value',
      type: 'money',
      sortable: true,
      formatter: (row) => row.currentType === 2 ? (row.totalOnSell ?? row.totalCurrentValue) : row.totalCurrentValue,
    },
    { key: 'totalGainLoss', header: 'P&L', type: 'pnl', sortable: true, percentKey: 'gainLossPercent' },
    { key: 'holdDays', header: 'Days', type: 'number', sortable: true, align: 'center' },
    { key: 'purchaseDate', header: 'Purchase', type: 'date', sortable: true },
    { key: 'exitDate', header: 'Exit', type: 'date', sortable: true },
    {
      key: 'actions',
      header: '',
      type: 'actions',
      canToggle: false,
      width: '96px',
      actions: [
        {
          icon: 'bi-pencil',
          label: 'Edit',
          color: 'outline-primary',
          click: (row) => this.openLedgerEdit(row),
        },
        {
          icon: 'bi-trash',
          label: 'Delete',
          color: 'outline-danger',
          click: (row) => { void this.deleteLedgerRow(row); },
        },
      ],
    },
  ]);


onStatusChange(value: number | string | null): void {
  this.filterStatus.set(value === '' || value === null ? null : Number(value));
  this.holdingsQuery.update((q) => ({ ...q, pageNumber: 1 }));
}

onExchangeChange(value: number | string | null): void {
  this.filterExchange.set(value === '' || value === null ? null : Number(value));
  this.holdingsQuery.update((q) => ({ ...q, pageNumber: 1 }));
  this.soldQuery.update((q) => ({ ...q, pageNumber: 1 }));
  this.ledgerQuery.update((q) => ({ ...q, pageNumber: 1 }));
}

  onHoldingsQuery(next: QueryRequest): void { this.holdingsQuery.set(next); }
  onSoldQuery(next: QueryRequest): void { this.soldQuery.set(next); }
  onLedgerQuery(next: QueryRequest): void { this.ledgerQuery.set(next); }
  onHoldingColumns(cols: TableColumn<PortfolioHolding>[]): void { this.holdingColumns.set(cols); }
  onSoldColumns(cols: TableColumn<PortfolioSold>[]): void { this.soldColumns.set(cols); }
  onLedgerColumns(cols: TableColumn<PortfolioLedgerItem>[]): void { this.ledgerColumns.set(cols); }

  ngOnInit(): void {
    this.loadData();
  }

  onHeaderAction(actionId: string): void {
    if (actionId === 'refresh') this.loadData();
    if (actionId === 'create') this.openCreatePortfolioModal();
    if (actionId === 'edit') this.openEditPortfolioModal();
    if (actionId === 'buy') this.openBuyModal();
    if (actionId === 'export') {
      if (this.activeTab() === 'sold') this.exportSoldHistory();
      else if (this.activeTab() === 'ledger') this.exportLedger();
      else this.exportHoldings();
    }
    if (actionId === 'import') {
      const kind: CsvImportKind =
        this.activeTab() === 'sold' ? 'sell' : this.activeTab() === 'ledger' ? 'buy' : 'buy';
      this.openImportModal(kind);
    }
  }

  loadData(): void {
    this.loading.set(true);
    this.portfolioService.getPortfolio().subscribe({
      next: (data) => {
        this.portfolio.set(data);
        if (!data?.id) {
          this.summary.set(null);
          this.holdings.set([]);
          this.soldHistory.set([]);
          this.ledger.set([]);
          this.loading.set(false);
          return;
        }
        this.portfolioService.getSummary().subscribe({
          next: (summary) => this.summary.set(summary),
          error: (err) => this.logger.error('Failed to load summary', err, LOG_FILE),
        });
        this.portfolioService.getHoldings().subscribe({
          next: (holdings) => {
            this.holdings.set(holdings);
            this.loading.set(false);
          },
          error: (err) => {
            this.logger.error('Failed to load holdings', err, LOG_FILE);
            this.loading.set(false);
            this.toast.error('Failed to load portfolio');
          },
        });
        this.loadSoldHistory();
        this.loadLedger();
      },
      error: (err) => {
        this.logger.error('Failed to load portfolio', err, LOG_FILE);
        this.loading.set(false);
        this.toast.error('Failed to load portfolio');
      },
    });
  }

  loadSoldHistory(): void {
    this.soldLoading.set(true);
    this.portfolioService.getSoldHistory().subscribe({
      next: (data) => {
        this.soldHistory.set(data);
        this.soldLoading.set(false);
      },
      error: (err) => {
        this.logger.error('Failed to load sold history', err, LOG_FILE);
        this.soldLoading.set(false);
      },
    });
  }

  setTab(tab: 'holdings' | 'sold' | 'ledger'): void {
    this.activeTab.set(tab);
    this.holdingsQuery.update((q) => ({ ...q, pageNumber: 1 }));
    this.soldQuery.update((q) => ({ ...q, pageNumber: 1 }));
    this.ledgerQuery.update((q) => ({ ...q, pageNumber: 1 }));
    if (tab === 'ledger') this.loadLedger();
  }

  setLedgerFilter(filter: PortfolioLedgerFilter): void {
    this.ledgerFilter.set(filter);
    this.loadLedger();
  }

  loadLedger(): void {
    if (!this.hasPortfolio()) {
      this.ledger.set([]);
      return;
    }
    this.ledgerLoading.set(true);
    this.portfolioService.getLedger(this.ledgerFilter()).subscribe({
      next: (data) => {
        this.ledger.set(data);
        this.ledgerLoading.set(false);
      },
      error: (err) => {
        this.logger.error('Failed to load ledger', err, LOG_FILE);
        this.ledgerLoading.set(false);
        this.toast.error('Failed to load ledger');
      },
    });
  }

  openCreatePortfolioModal(): void {
    this.isEditPortfolio.set(false);
    this.portfolioAttempted.set(false);
    this.portfolioNameTouched.set(false);
    this.portfolioForm = { name: 'My Portfolio', description: '' };
    this.showPortfolioModal.set(true);
  }

  openEditPortfolioModal(): void {
    const current = this.portfolio();
    this.isEditPortfolio.set(true);
    this.portfolioAttempted.set(false);
    this.portfolioNameTouched.set(false);
    this.portfolioForm = {
      name: current?.name || 'My Portfolio',
      description: current?.description || '',
    };
    this.showPortfolioModal.set(true);
  }

  closePortfolioModal(): void {
    this.showPortfolioModal.set(false);
    this.portfolioAttempted.set(false);
    this.portfolioNameTouched.set(false);
  }

  submitPortfolio(): void {
    this.portfolioAttempted.set(true);
    const nameError = this.portfolioNameError();
    if (nameError) {
      this.toast.warning(nameError);
      return;
    }
    const name = this.portfolioForm.name.trim();
    this.portfolioSaving.set(true);

    if (this.isEditPortfolio()) {
      const request: UpdatePortfolioRequest = {
        name,
        description: this.portfolioForm.description?.trim() || undefined,
        isActive: true,
      };
      this.portfolioService.update(request).subscribe({
        next: (res) => {
          this.portfolioSaving.set(false);
          if (res?.success && res.data) {
            this.portfolio.set(res.data);
            this.toast.success('Portfolio updated');
            this.closePortfolioModal();
            this.loadData();
          } else {
            this.toast.error(apiErrorMessage(res, 'Update failed'));
          }
        },
        error: (err) => {
          this.portfolioSaving.set(false);
          this.logger.error('Update portfolio error', err, LOG_FILE);
          this.toast.error(apiErrorMessage(err, 'Update failed'));
        },
      });
      return;
    }

    const request: CreatePortfolioRequest = {
      name,
      description: this.portfolioForm.description?.trim() || undefined,
    };
    this.portfolioService.create(request).subscribe({
      next: (res) => {
        this.portfolioSaving.set(false);
        if (res?.success && res.data) {
          this.portfolio.set(res.data);
          this.toast.success('Portfolio created');
          this.closePortfolioModal();
          this.loadData();
        } else {
          this.toast.error(apiErrorMessage(res, 'Create failed'));
        }
      },
      error: (err) => {
        this.portfolioSaving.set(false);
        this.logger.error('Create portfolio error', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Create failed'));
      },
    });
  }

  clearFilters(): void {
    this.filterStatus.set(null);
    this.filterExchange.set(null);
    this.filterFromDate.set('');
    this.filterToDate.set('');
    this.searchTerm.set('');
    this.holdingsQuery.update((q) => ({ ...q, pageNumber: 1, globalSearch: null }));
    this.soldQuery.update((q) => ({ ...q, pageNumber: 1, globalSearch: null }));
    this.ledgerQuery.update((q) => ({ ...q, pageNumber: 1, globalSearch: null }));
  }

  openBuyModal(): void {
    if (!this.hasPortfolio()) {
      this.openCreatePortfolioModal();
      this.toast.info('Create a portfolio first');
      return;
    }
    this.editingHoldId.set(null);
    this.holdSoldQuantity.set(0);
    this.buyAttempted.set(false);
    this.buyTouched.set({ stock: false, quantity: false, price: false, date: false });
    this.buyForm = {
      stockId: 0,
      symbol: '',
      companyName: '',
      logoUrl: null,
      quantity: 1,
      purchasePrice: 0,
      purchaseDate: new Date().toISOString().substring(0, 10),
      exchange: 1,
      notes: '',
    };
    this.stockSearch.set('');
    this.stockOptions.set([]);
    this.showBuyModal.set(true);
  }

  openEditHold(hold: PortfolioHolding): void {
    this.editingHoldId.set(hold.id);
    this.holdSoldQuantity.set(Math.max(0, hold.quantity - hold.remainingQuantity));
    this.buyAttempted.set(false);
    this.buyTouched.set({ stock: false, quantity: false, price: false, date: false });
    this.buyForm = {
      stockId: hold.stockId,
      symbol: hold.symbol,
      companyName: hold.companyName,
      logoUrl: hold.logoUrl ?? null,
      quantity: hold.quantity,
      purchasePrice: hold.purchasePrice,
      purchaseDate: (hold.purchaseDate || '').substring(0, 10),
      exchange: hold.exchange,
      notes: hold.holdNotes ?? '',
    };
    this.stockSearch.set('');
    this.stockOptions.set([]);
    this.showBuyModal.set(true);
  }

  onPriceFocus(event: Event): void {
    const el = event.target as HTMLInputElement;
    const n = Number(el.value);
    if (!Number.isFinite(n) || n === 0) el.value = '';
  }

  onBuyPriceInput(event: Event): void {
    const el = event.target as HTMLInputElement;
    const raw = el.value.trim();
    if (raw === '' || raw === '.') return;
    const n = Number(raw);
    if (!Number.isFinite(n) || n < 0) return;
    this.buyForm.purchasePrice = n;
  }

  onBuyPriceBlur(event: Event): void {
    const el = event.target as HTMLInputElement;
    let n = Number(el.value);
    if (!Number.isFinite(n) || n < 0) n = 0;
    this.buyForm.purchasePrice = Number(n.toFixed(2));
    el.value = this.buyForm.purchasePrice > 0 ? this.buyForm.purchasePrice.toFixed(2) : '0';
    this.touchBuy('price');
  }

  onSellPriceInput(event: Event): void {
    const el = event.target as HTMLInputElement;
    const raw = el.value.trim();
    if (raw === '' || raw === '.') return;
    const n = Number(raw);
    if (!Number.isFinite(n) || n < 0) return;
    this.sellForm.sellPrice = n;
  }

  onSellPriceBlur(event: Event): void {
    const el = event.target as HTMLInputElement;
    let n = Number(el.value);
    if (!Number.isFinite(n) || n < 0) n = 0;
    this.sellForm.sellPrice = Number(n.toFixed(2));
    el.value = this.sellForm.sellPrice > 0 ? this.sellForm.sellPrice.toFixed(2) : '0';
    this.touchSell('price');
  }

  onQtyFocus(event: Event): void {
    const el = event.target as HTMLInputElement;
    if (Number(el.value) === 1) el.value = '';
  }

  onBuyQty(event: Event): void {
    const el = event.target as HTMLInputElement;
    const raw = this.stripLeadingZeros(el);
    if (raw === '') return;
    const n = Number(raw);
    if (!Number.isFinite(n) || n < 0) return;
    this.buyForm.quantity = Math.trunc(n);
  }

  onBuyQtyBlur(event: Event): void {
    const el = event.target as HTMLInputElement;
    let n = Math.trunc(Number(el.value));
    const min = this.isEditingHold() ? Math.max(1, this.holdSoldQuantity()) : 1;
    if (!Number.isFinite(n) || n < min) n = min;
    this.buyForm.quantity = n;
    el.value = String(n);
    this.touchBuy('quantity');
  }

  closeBuyModal(): void {
    this.showBuyModal.set(false);
    this.editingHoldId.set(null);
    this.holdSoldQuantity.set(0);
    this.buyAttempted.set(false);
    this.buyTouched.set({ stock: false, quantity: false, price: false, date: false });
  }

  onStockSearch(term: string): void {
    this.stockSearch.set(term);
    if (term.trim().length < 2) {
      this.stockOptions.set([]);
      return;
    }
    this.stockSearching.set(true);
    const request: QueryRequest = {
      pageNumber: 1,
      pageSize: 8,
      globalSearch: term,
      filters: [],
      sorts: [],
    };
    this.stockService.search(request).subscribe({
      next: (res) => {
        this.stockOptions.set(res.data ?? []);
        this.stockSearching.set(false);
      },
      error: () => {
        this.stockOptions.set([]);
        this.stockSearching.set(false);
      },
    });
  }

  selectStock(stock: Stock): void {
    this.buyForm.stockId = stock.stockId || stock.id;
    this.buyForm.symbol = stock.symbol;
    this.buyForm.companyName = stock.companyName;
    this.buyForm.logoUrl = stock.logoUrl ?? null;
    this.buyForm.purchasePrice = stock.currentPrice || 0;
    this.stockSearch.set(`${stock.symbol} - ${stock.companyName}`);
    this.stockOptions.set([]);
    this.touchBuy('stock');
  }

  submitBuy(): void {
    this.buyAttempted.set(true);
    const firstError =
      this.buyFieldError('stock') ||
      this.buyFieldError('quantity') ||
      this.buyFieldError('price') ||
      this.buyFieldError('date');
    if (firstError) {
      this.toast.warning(firstError);
      return;
    }
    this.buySaving.set(true);
    const editId = this.editingHoldId();
    if (editId !== null) {
      const request: UpdateHoldRequest = {
        quantity: this.buyForm.quantity,
        purchasePrice: this.buyForm.purchasePrice,
        purchaseDate: this.buyForm.purchaseDate,
        exchange: this.buyForm.exchange,
        notes: this.buyForm.notes || undefined,
      };
      this.portfolioService.updateHold(editId, request).subscribe({
        next: (res) => {
          this.buySaving.set(false);
          if (res?.success) {
            this.toast.success('Buy lot updated');
            this.closeBuyModal();
            this.loadData();
          } else {
            this.toast.error(apiErrorMessage(res, 'Update failed'));
          }
        },
        error: (err) => {
          this.buySaving.set(false);
          this.logger.error('Update hold error', err, LOG_FILE);
          this.toast.error(apiErrorMessage(err, 'Update failed'));
        },
      });
      return;
    }
    const request: BuyStockRequest = {
      stockId: this.buyForm.stockId,
      quantity: this.buyForm.quantity,
      purchasePrice: this.buyForm.purchasePrice,
      purchaseDate: this.buyForm.purchaseDate,
      exchange: this.buyForm.exchange,
      notes: this.buyForm.notes || undefined,
    };
    this.portfolioService.buy(request).subscribe({
      next: (res) => {
        this.buySaving.set(false);
        if (res) {
          this.toast.success(`Bought ${request.quantity} of ${this.buyForm.symbol}`);
          this.closeBuyModal();
          this.loadData();
        } else {
          this.toast.error(apiErrorMessage(res, 'Buy failed'));
        }
      },
      error: (err) => {
        this.buySaving.set(false);
        this.logger.error('Buy error', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Buy failed'));
      },
    });
  }

  async deleteHold(hold: PortfolioHolding): Promise<void> {
    const soldQty = Math.max(0, hold.quantity - hold.remainingQuantity);
    const ok = await this.confirm.open({
      title: 'Delete buy lot',
      message: soldQty > 0
        ? `Delete ${hold.quantity} ${hold.symbol} bought on ${(hold.purchaseDate || '').substring(0, 10)}? This also removes ${soldQty} sold share(s) tied to this lot.`
        : `Delete this ${hold.symbol} lot of ${hold.quantity}? This cannot be undone.`,
      confirmText: 'Delete',
      confirmColor: 'danger',
    });
    if (!ok) return;
    this.portfolioService.deleteHold(hold.id).subscribe({
      next: (res) => {
        if (res?.success) {
          this.toast.success(res.message || 'Buy lot deleted');
          this.loadData();
        } else {
          this.toast.error(apiErrorMessage(res, 'Delete failed'));
        }
      },
      error: (err) => {
        this.logger.error('Delete hold error', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Delete failed'));
      },
    });
  }

  openHoldingDetail(stockId: number): void {
    void this.router.navigate(['/holdings', stockId]);
  }

  openSellModal(hold: PortfolioHolding): void {
    this.selectedHold.set(hold);
    this.sellAttempted.set(false);
    this.sellTouched.set({ quantity: false, price: false, date: false });
    const availableQty = this.holdings()
      .filter((h) => h.stockId === hold.stockId)
      .reduce((sum, h) => sum + h.remainingQuantity, 0);
    this.sellAvailableQty.set(availableQty);
    this.sellForm = {
      sellQuantity: availableQty,
      sellPrice: hold.currentPrice || hold.purchasePrice,
      soldDate: new Date().toISOString().substring(0, 10),
      notes: '',
    };
    this.showSellModal.set(true);
  }

  onSellQty(event: Event): void {
    const el = event.target as HTMLInputElement;
    const max = Math.max(1, this.sellAvailableQty());
    const raw = this.stripLeadingZeros(el);
    if (raw === '') return;
    let n = Math.trunc(Number(raw));
    if (!Number.isFinite(n)) return;
    if (n > max) {
      n = max;
      el.value = String(n);
    }
    this.sellForm.sellQuantity = n;
  }

  onSellQtyBlur(event: Event): void {
    const el = event.target as HTMLInputElement;
    const max = Math.max(1, this.sellAvailableQty());
    let n = Math.trunc(Number(el.value));
    if (!Number.isFinite(n) || n < 1) n = 1;
    if (n > max) n = max;
    this.sellForm.sellQuantity = n;
    el.value = String(n);
    this.touchSell('quantity');
  }

  private stripLeadingZeros(el: HTMLInputElement): string {
    const next = el.value.replace(/^0+/, '');
    if (next !== el.value) el.value = next;
    return next.trim();
  }

  closeSellModal(): void {
    this.showSellModal.set(false);
    this.selectedHold.set(null);
    this.sellAvailableQty.set(0);
    this.sellAttempted.set(false);
    this.sellTouched.set({ quantity: false, price: false, date: false });
  }

  submitSell(): void {
    const hold = this.selectedHold();
    if (!hold) return;
    this.sellAttempted.set(true);
    const firstError =
      this.sellFieldError('quantity') ||
      this.sellFieldError('price') ||
      this.sellFieldError('date');
    if (firstError) {
      this.toast.warning(firstError);
      return;
    }
    this.sellSaving.set(true);
    const request: SellStockRequest = {
      stockId: hold.stockId,
      sellQuantity: this.sellForm.sellQuantity,
      sellPrice: this.sellForm.sellPrice,
      soldDate: this.sellForm.soldDate,
      notes: this.sellForm.notes || undefined,
    };
    this.portfolioService.sell(request).subscribe({
      next: (res) => {
        this.sellSaving.set(false);
        if (res?.success && res.data) {
          this.toast.success(`Sold ${res.data.totalSellQuantity} of ${res.data.symbol}`);
          this.closeSellModal();
          this.loadData();
        } else {
          this.toast.error(apiErrorMessage(res, 'Sell failed'));
        }
      },
      error: (err) => {
        this.sellSaving.set(false);
        this.logger.error('Sell error', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Sell failed'));
      },
    });
  }

  openEditSold(sold: PortfolioSold): void {
    const hold = this.holdings().find((h) => h.id === sold.holdId);
    const max = sold.sellQuantity + (hold?.remainingQuantity ?? 0);
    this.selectedSold.set(sold);
    this.editSoldMaxQty.set(Math.max(1, max));
    this.editSoldAttempted.set(false);
    this.editSoldTouched.set({ quantity: false, price: false, date: false });
    this.editSoldForm = {
      sellQuantity: sold.sellQuantity,
      sellPrice: sold.sellPrice,
      soldDate: (sold.soldDate || '').substring(0, 10),
      notes: sold.soldNotes ?? '',
    };
    this.showEditSoldModal.set(true);
  }

  closeEditSoldModal(): void {
    this.showEditSoldModal.set(false);
    this.selectedSold.set(null);
    this.editSoldMaxQty.set(0);
    this.editSoldAttempted.set(false);
    this.editSoldTouched.set({ quantity: false, price: false, date: false });
  }

  onEditSoldQty(event: Event): void {
    const el = event.target as HTMLInputElement;
    const max = Math.max(1, this.editSoldMaxQty());
    const raw = this.stripLeadingZeros(el);
    if (raw === '') return;
    let n = Math.trunc(Number(raw));
    if (!Number.isFinite(n)) return;
    if (n > max) {
      n = max;
      el.value = String(n);
    }
    this.editSoldForm.sellQuantity = n;
  }

  onEditSoldQtyBlur(event: Event): void {
    const el = event.target as HTMLInputElement;
    const max = Math.max(1, this.editSoldMaxQty());
    let n = Math.trunc(Number(el.value));
    if (!Number.isFinite(n) || n < 1) n = 1;
    if (n > max) n = max;
    this.editSoldForm.sellQuantity = n;
    el.value = String(n);
    this.touchEditSold('quantity');
  }

  onEditSoldPriceInput(event: Event): void {
    const el = event.target as HTMLInputElement;
    const raw = el.value.trim();
    if (raw === '' || raw === '.') return;
    const n = Number(raw);
    if (!Number.isFinite(n) || n < 0) return;
    this.editSoldForm.sellPrice = n;
  }

  onEditSoldPriceBlur(event: Event): void {
    const el = event.target as HTMLInputElement;
    let n = Number(el.value);
    if (!Number.isFinite(n) || n < 0) n = 0;
    this.editSoldForm.sellPrice = Number(n.toFixed(2));
    el.value = this.editSoldForm.sellPrice > 0 ? this.editSoldForm.sellPrice.toFixed(2) : '0';
    this.touchEditSold('price');
  }

  submitEditSold(): void {
    const sold = this.selectedSold();
    if (!sold) return;
    this.editSoldAttempted.set(true);
    const firstError =
      this.editSoldFieldError('quantity') ||
      this.editSoldFieldError('price') ||
      this.editSoldFieldError('date');
    if (firstError) {
      this.toast.warning(firstError);
      return;
    }
    this.editSoldSaving.set(true);
    const request: UpdateSoldRequest = {
      sellQuantity: this.editSoldForm.sellQuantity,
      sellPrice: this.editSoldForm.sellPrice,
      soldDate: this.editSoldForm.soldDate,
      notes: this.editSoldForm.notes || undefined,
    };
    this.portfolioService.updateSold(sold.id, request).subscribe({
      next: (res) => {
        this.editSoldSaving.set(false);
        if (res?.success) {
          this.toast.success('Sell record updated');
          this.closeEditSoldModal();
          this.loadData();
        } else {
          this.toast.error(apiErrorMessage(res, 'Update failed'));
        }
      },
      error: (err) => {
        this.editSoldSaving.set(false);
        this.logger.error('Update sold error', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Update failed'));
      },
    });
  }

  async deleteSold(sold: PortfolioSold): Promise<void> {
    const ok = await this.confirm.open({
      title: 'Delete sell',
      message: `Remove the sale of ${sold.sellQuantity} ${sold.symbol} on ${(sold.soldDate || '').substring(0, 10)}? Quantity is restored to the buy lot.`,
      confirmText: 'Delete',
      confirmColor: 'danger',
    });
    if (!ok) return;
    this.portfolioService.deleteSold(sold.id).subscribe({
      next: (res) => {
        if (res?.success) {
          this.toast.success(res.message || 'Sell deleted');
          this.loadData();
        } else {
          this.toast.error(apiErrorMessage(res, 'Delete failed'));
        }
      },
      error: (err) => {
        this.logger.error('Delete sold error', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Delete failed'));
      },
    });
  }

  openLedgerEdit(row: PortfolioLedgerItem): void {
    if (row.currentType === 2) {
      const sold = this.soldHistory().find((s) => s.id === (row.soldId ?? row.id));
      if (!sold) {
        this.toast.warning('Sell record not found');
        return;
      }
      this.openEditSold(sold);
      return;
    }
    const hold = this.holdings().find((h) => h.id === (row.holdId ?? row.id));
    if (!hold) {
      this.toast.warning('Buy lot not found');
      return;
    }
    this.openEditHold(hold);
  }

  async deleteLedgerRow(row: PortfolioLedgerItem): Promise<void> {
    if (row.currentType === 2) {
      const sold = this.soldHistory().find((s) => s.id === (row.soldId ?? row.id));
      if (!sold) {
        this.toast.warning('Sell record not found');
        return;
      }
      await this.deleteSold(sold);
      return;
    }
    const hold = this.holdings().find((h) => h.id === (row.holdId ?? row.id));
    if (!hold) {
      this.toast.warning('Buy lot not found');
      return;
    }
    await this.deleteHold(hold);
  }

  exportHoldings(): void {
    const data = this.filteredHoldings();
    if (data.length === 0) {
      this.toast.warning('No holdings to export');
      return;
    }
    const headers = ['Company', 'Symbol', 'Qty', 'Avg Buy', 'Market Price', 'Investment', 'Current Value', 'P&L', 'P&L %', 'Days', 'Status', 'Purchase Date'];
    const rows = data.map((h) => [h.companyName, h.symbol, h.remainingQuantity, h.purchasePrice, h.currentPrice, h.remainingInvestment, h.currentValue, h.unrealizedGainLoss, h.unrealizedGainLossPercent, h.holdDays ?? 0, this.getLotStatusBadge(h.lotStatus).text, h.purchaseDate]);
    this.downloadCsv('portfolio-holdings.csv', headers, rows);
    this.toast.success('Holdings exported successfully');
  }

  exportSoldHistory(): void {
    const data = this.filteredSoldHistory();
    if (data.length === 0) {
      this.toast.warning('No sold history to export');
      return;
    }
    const headers = ['Company', 'Symbol', 'Qty', 'Buy Price', 'Sell Price', 'Cost', 'Sell Amount', 'P&L', 'P&L %', 'Days Held', 'Sold Date'];
    const rows = data.map((s) => [s.companyName, s.symbol, s.sellQuantity, s.purchasePrice, s.sellPrice, s.costAmount, s.sellAmount, s.realizedGainLoss, s.realizedGainLossPercent, s.holdDays ?? 0, s.soldDate]);
    this.downloadCsv('portfolio-sold-history.csv', headers, rows);
    this.toast.success('Sold history exported successfully');
  }

  exportLedger(): void {
    const data = this.ledger();
    if (data.length === 0) {
      this.toast.warning('No ledger rows to export');
      return;
    }
    const headers = ['S.No', 'Company', 'Symbol', 'Type', 'Qty', 'Buy Price', 'Market / Sell', 'Investment', 'Current / Sell Amt', 'P&L', 'P&L %', 'Days', 'Purchase Date', 'Exit Date'];
    const rows = data.map((r) => [r.serialNo, r.companyName, r.symbol, this.getActionLabel(r.currentType), r.netQuantity, r.purchasePrice, r.currentType === 2 ? (r.sellPrice ?? r.marketPrice) : r.marketPrice, r.totalInvestment, r.currentType === 2 ? (r.totalOnSell ?? r.totalCurrentValue) : r.totalCurrentValue, r.totalGainLoss, r.gainLossPercent, r.holdDays, r.purchaseDate, r.exitDate ?? '']);
    this.downloadCsv(`portfolio-ledger-${this.ledgerFilter()}.csv`, headers, rows);
    this.toast.success('Ledger exported successfully');
  }

  private downloadCsv(filename: string, headers: string[], rows: (string | number)[][]): void {
    const csvContent = [
      headers.join(','),
      ...rows.map((row) =>
        row
          .map((cell) => {
            const value = cell?.toString() ?? '';
            return value.includes(',') || value.includes('"') ? `"${value.replace(/"/g, '""')}"` : value;
          })
          .join(','),
      ),
    ].join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
  }

  openImportModal(kind: CsvImportKind = 'buy'): void {
    this.importKind.set(kind);
    this.importPreview.set([]);
    this.importErrors.set([]);
    this.importFileName.set('');
    this.showImportModal.set(true);
  }

  setImportKind(kind: CsvImportKind): void {
    this.importKind.set(kind);
    this.importPreview.set([]);
    this.importErrors.set([]);
    this.importFileName.set('');
  }

  closeImportModal(): void {
    this.showImportModal.set(false);
    this.importPreview.set([]);
    this.importErrors.set([]);
    this.importFileName.set('');
  }

  downloadImportSample(): void {
    const kind = this.importKind();
    downloadTextFile(`import-${kind}-sample.csv`, sampleCsv(kind));
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.importFileName.set(file.name);
    const reader = new FileReader();
    reader.onload = () => {
      const parsed = parseImportCsv(reader.result as string, this.importKind());
      this.importPreview.set(parsed.rows);
      this.importErrors.set(parsed.errors);
      if (parsed.rows.length === 0) {
        this.toast.warning(parsed.errors[0] || 'No valid rows found in CSV');
      }
    };
    reader.readAsText(file);
    input.value = '';
  }

  importTitle(): string {
    switch (this.importKind()) {
      case 'sell': return 'Import sells';
      case 'dividend': return 'Import dividends';
      default: return 'Import buys';
    }
  }

  importSubtitle(): string {
    switch (this.importKind()) {
      case 'sell': return 'FIFO sell — oldest lots of each symbol are consumed first.';
      case 'dividend': return 'Credits matched by symbol. Stock must already have a buy.';
      default: return 'CSV of buys — matched by symbol.';
    }
  }

  importConfirmLabel(): string {
    const n = this.importPreview().length;
    switch (this.importKind()) {
      case 'sell': return `Import ${n} sell${n === 1 ? '' : 's'}`;
      case 'dividend': return `Import ${n} dividend${n === 1 ? '' : 's'}`;
      default: return `Import ${n} holding${n === 1 ? '' : 's'}`;
    }
  }

  private async resolveStockId(symbol: string): Promise<number | null> {
    const searchReq: QueryRequest = {
      pageNumber: 1,
      pageSize: 8,
      globalSearch: symbol,
      filters: [],
      sorts: [],
    };
    const searchRes = await firstValueFrom(this.stockService.search(searchReq));
    const stock = searchRes?.data?.find((s) => s.symbol.toLowerCase() === symbol.toLowerCase());
    return stock ? (stock.id || stock.stockId) : null;
  }

  async submitImport(): Promise<void> {
    const rows = this.importPreview();
    if (rows.length === 0) return;
    this.importSaving.set(true);
    let successCount = 0;
    let failCount = 0;
    const kind = this.importKind();

    for (const row of rows) {
      try {
        const stockId = await this.resolveStockId(row.symbol);
        if (!stockId) {
          failCount++;
          continue;
        }

        if (kind === 'sell') {
          const request: SellStockRequest = {
            stockId,
            sellQuantity: row.quantity,
            sellPrice: row.price,
            soldDate: row.date,
            notes: row.notes || 'Imported from CSV',
          };
          const result = await firstValueFrom(this.portfolioService.sell(request));
          if (result?.success) successCount++;
          else failCount++;
        } else if (kind === 'dividend') {
          const result = await firstValueFrom(
            this.portfolioService.addDividend({
              stockId,
              quantity: row.quantity,
              perShareAmount: row.price,
              amount: row.amount,
              dividendDate: row.date,
              exDate: row.exDate,
              recordDate: row.recordDate,
              notes: row.notes || 'Imported from CSV',
            }),
          );
          if (result?.success) successCount++;
          else failCount++;
        } else {
          const request: BuyStockRequest = {
            stockId,
            quantity: row.quantity,
            purchasePrice: row.price,
            purchaseDate: row.date,
            exchange: 1,
            notes: row.notes || 'Imported from CSV',
          };
          const result = await firstValueFrom(this.portfolioService.buy(request));
          if (result) successCount++;
          else failCount++;
        }
      } catch {
        failCount++;
      }
    }

    this.importSaving.set(false);
    this.closeImportModal();
    this.loadData();
    if (successCount > 0) this.toast.success(`Imported ${successCount} row(s)`);
    if (failCount > 0) this.toast.warning(`${failCount} row(s) failed (unknown symbol, no lot, or validation)`);
  }

  getLotStatusBadge(status: number): { text: string; class: string } {
    switch (status) {
      case 1: return { text: 'Open', class: 'bg-success' };
      case 2: return { text: 'Partial', class: 'bg-primary' };
      case 3: return { text: 'Sold', class: 'bg-secondary' };
      default: return { text: 'Unknown', class: 'bg-dark' };
    }
  }

  formatPnL(value: number): string {
    const sign = value >= 0 ? '+' : '';
    return `${sign}${value.toFixed(2)}`;
  }

  getActionLabel(type: number): string {
    return type === 2 ? 'Sell' : 'Hold';
  }

  getActionBadge(type: number): { text: string; class: string } {
    return type === 2
      ? { text: 'Sell', class: 'bg-danger-subtle text-danger' }
      : { text: 'Hold', class: 'bg-success-subtle text-success' };
  }

  ledgerFilterLabel(filter: PortfolioLedgerFilter): string {
    if (filter === 'hold') return 'Hold';
    if (filter === 'sell') return 'Sell';
    return 'Lifetime';
  }
}