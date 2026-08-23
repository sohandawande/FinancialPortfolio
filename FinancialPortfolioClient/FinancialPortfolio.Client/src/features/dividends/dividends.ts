import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { PageHeader } from '../../layout/components/page-header/page-header';
import { StockLogo } from '../../layout/components/stock-logo/stock-logo';
import { FpModal } from '../../layout/components/fp-modal/fp-modal';
import { FpDate } from '../../layout/components/fp-date/fp-date';
import { DataGrid } from '../../layout/components/table/data-grid/data-grid';
import { PageHeaderAction } from '../../core/models/page-header/page-header-action.model';
import { TableColumn } from '../../core/models/query/table-column.model';
import { QueryRequest } from '../../core/models/query/query-request.model';
import { PortfolioService } from '../../core/services/portfolio/portfolio.service';
import { StockService } from '../../core/services/stock/stock.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../core/services/confirm-modal/confirm-modal.service';
import { ClientLoggerService } from '../../core/services/logs/client-logger.service';
import { isFutureIsoDate } from '../../core/helper/validators/app.validators';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import { PortfolioDividend } from '../../core/models/portfolio/portfolio-dividend.model';
import {
  DividendListView,
  PortfolioDividendOverview,
} from '../../core/models/portfolio/portfolio-dividend-overview.model';
import { PortfolioDividendStockGroup } from '../../core/models/portfolio/portfolio-dividend-stock-group.model';
import { PortfolioDividendYearGroup } from '../../core/models/portfolio/portfolio-dividend-year-group.model';
import { AddDividendRequest } from '../../core/models/portfolio/add-dividend-request.model';
import { UpdateDividendRequest } from '../../core/models/portfolio/update-dividend-request.model';
import { PortfolioPosition } from '../../core/models/portfolio/portfolio-position.model';
import {
  CsvImportRow,
  downloadTextFile,
  parseImportCsv,
  sampleCsv,
} from '../../core/utils/csv.util';

const LOG_FILE = 'dividends.ts';

function toDateInput(value?: string | null): string {
  if (!value) return '';
  return value.substring(0, 10);
}

@Component({
  selector: 'app-dividends',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeader, StockLogo, FpModal, DataGrid, CurrencyPipe, DatePipe, FpDate],
  templateUrl: './dividends.html',
  styleUrls: ['./dividends.css', '../../ui-styles/components/import-modal.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dividends implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly portfolioService = inject(PortfolioService);
  private readonly stockService = inject(StockService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly logger = inject(ClientLoggerService);

  readonly overview = signal<PortfolioDividendOverview | null>(null);
  readonly positions = signal<PortfolioPosition[]>([]);
  readonly loading = signal(true);
  readonly view = signal<DividendListView>('stocks');
  readonly searchTerm = signal('');
  readonly payoutQuery = signal<QueryRequest>({ pageNumber: 1, pageSize: 10 });
  readonly searchKeys = ['symbol', 'companyName', 'notes'];
  readonly payoutQueryView = computed<QueryRequest>(() => ({
    ...this.payoutQuery(),
    globalSearch: this.searchTerm(),
  }));
  readonly expandedStockIds = signal<Set<number>>(new Set());
  readonly expandedYears = signal<Set<number>>(new Set());
  readonly focusStockId = signal<number | null>(null);

  readonly showDividendModal = signal(false);
  readonly showImportModal = signal(false);
  readonly importSaving = signal(false);
  readonly importPreview = signal<CsvImportRow[]>([]);
  readonly importErrors = signal<string[]>([]);
  readonly importFileName = signal('');
  readonly savingDividend = signal(false);
  readonly editingDividendId = signal<number | null>(null);
  readonly formStockId = signal(0);

  dividendForm = {
    stockId: 0,
    quantity: 1,
    perShareAmount: 0,
    amount: 0,
    dividendDate: new Date().toISOString().substring(0, 10),
    exDate: '',
    recordDate: '',
    notes: '',
  };

  readonly isEditing = computed(() => this.editingDividendId() !== null);
  readonly dividendAttempted = signal(false);
  readonly dividendTouched = signal({ stock: false, quantity: false, perShare: false, date: false });

  touchDividend(field: 'stock' | 'quantity' | 'perShare' | 'date'): void {
    this.dividendTouched.update((t) => ({ ...t, [field]: true }));
  }

  dividendFieldError(field: 'stock' | 'quantity' | 'perShare' | 'date'): string {
    if (!(this.dividendAttempted() || this.dividendTouched()[field])) return '';
    if (field === 'stock' && !this.dividendForm.stockId) return 'Select a stock';
    if (field === 'quantity' && this.dividendForm.quantity < 1) return 'Quantity must be at least 1';
    if (field === 'perShare' && this.dividendForm.perShareAmount <= 0) return 'Enter a per-share amount greater than 0';
    if (field === 'date') {
      if (!this.dividendForm.dividendDate) return 'Credit date is required';
      if (isFutureIsoDate(this.dividendForm.dividendDate)) return 'Credit date cannot be in the future';
    }
    return '';
  }

  readonly headerActions = computed<PageHeaderAction[]>(() => [
    {
      id: 'holdings',
      label: 'Holdings',
      icon: 'bi-layers',
      color: 'outline-secondary',
    },
    {
      id: 'refresh',
      label: 'Refresh',
      icon: 'bi-arrow-clockwise',
      color: 'outline-secondary',
      disabled: this.loading(),
    },
    {
      id: 'import',
      label: 'Import',
      icon: 'bi-upload',
      color: 'outline-success',
      disabled: this.loading(),
    },
    {
      id: 'add',
      label: 'Add dividend',
      icon: 'bi-plus-lg',
      color: 'primary',
      disabled: this.loading(),
    },
  ]);

  readonly filteredStocks = computed<PortfolioDividendStockGroup[]>(() => {
    const stocks = this.overview()?.stocks ?? [];
    const focus = this.focusStockId();
    const term = this.searchTerm().trim().toLowerCase();
    return stocks.filter((s) => {
      if (focus && s.stockId !== focus) return false;
      if (!term) return true;
      return s.symbol.toLowerCase().includes(term) || s.companyName.toLowerCase().includes(term);
    });
  });

  readonly filteredPayouts = computed<PortfolioDividend[]>(() => {
    const payouts = this.overview()?.payouts ?? [];
    const focus = this.focusStockId();
    const term = this.searchTerm().trim().toLowerCase();
    return payouts.filter((p) => {
      if (focus && p.stockId !== focus) return false;
      if (!term) return true;
      return (
        p.symbol.toLowerCase().includes(term) ||
        p.companyName.toLowerCase().includes(term) ||
        (p.notes ?? '').toLowerCase().includes(term)
      );
    });
  });

  readonly filteredYears = computed<PortfolioDividendYearGroup[]>(() => {
    const years = this.overview()?.years ?? [];
    const focus = this.focusStockId();
    const term = this.searchTerm().trim().toLowerCase();

    return years
      .map((y) => ({
        ...y,
        payouts: y.payouts.filter((p) => {
          if (focus && p.stockId !== focus) return false;
          if (!term) return true;
          return p.symbol.toLowerCase().includes(term) || p.companyName.toLowerCase().includes(term);
        }),
      }))
      .map((y) => ({
        ...y,
        payoutCount: y.payouts.length,
        amount: Number(y.payouts.reduce((sum, p) => sum + p.amount, 0).toFixed(2)),
        companyCount: new Set(y.payouts.map((p) => p.stockId)).size,
      }))
      .filter((y) => y.payouts.length > 0);
  });

  readonly focusStock = computed(() => {
    const id = this.focusStockId();
    if (!id) return null;
    return this.overview()?.stocks.find((s) => s.stockId === id) ?? null;
  });

  readonly latestPayout = computed(() => this.overview()?.payouts[0] ?? null);

  readonly formStock = computed(() => {
    const id = this.formStockId();
    return this.positions().find((p) => p.stockId === id) ?? null;
  });

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      const stockId = Number(params.get('stockId'));
      this.focusStockId.set(stockId > 0 ? stockId : null);
      if (stockId > 0) {
        this.expandedStockIds.update((set) => new Set(set).add(stockId));
      }
    });
    this.load();
  }

  onHeaderAction(actionId: string): void {
    if (actionId === 'holdings') void this.router.navigate(['/holdings']);
    if (actionId === 'refresh') this.load();
    if (actionId === 'import') this.openImportModal();
    if (actionId === 'add') this.openAddModal();
  }

  readonly payoutColumns = signal<TableColumn<PortfolioDividend>[]>([
    { key: 'companyName', header: 'Company', type: 'stock', sortable: true },
    { key: 'quantity', header: 'Shares', type: 'number', sortable: true, align: 'end' },
    { key: 'perShareAmount', header: 'Per share', type: 'money', sortable: true },
    { key: 'amount', header: 'Amount', type: 'money', sortable: true },
    { key: 'dividendDate', header: 'Paid on', type: 'date', sortable: true },
    { key: 'exDate', header: 'Ex date', type: 'date', sortable: true },
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
          click: (row) => this.openEditModal(row),
        },
        {
          icon: 'bi-trash',
          label: 'Delete',
          color: 'outline-danger',
          click: (row) => { void this.deleteDividend(row.id); },
        },
      ],
    },
  ]);

  setView(view: DividendListView): void {
    this.view.set(view);
    this.payoutQuery.update((q) => ({ ...q, pageNumber: 1 }));
  }

  onPayoutQuery(next: QueryRequest): void {
    this.payoutQuery.set(next);
  }

  onPayoutColumns(cols: TableColumn<PortfolioDividend>[]): void {
    this.payoutColumns.set(cols);
  }


  load(): void {
    this.loading.set(true);
    this.portfolioService.getDividendOverview().subscribe({
      next: (data) => {
        this.overview.set(
          data ?? {
            totalAmount: 0,
            companyCount: 0,
            payoutCount: 0,
            stocks: [],
            years: [],
            payouts: [],
          },
        );
        this.loading.set(false);
        const focus = this.focusStockId();
        if (focus) {
          this.expandedStockIds.update((set) => new Set(set).add(focus));
        }
      },
      error: (err) => {
        this.logger.error('Failed to load dividend overview', err, LOG_FILE);
        this.loading.set(false);
        this.toast.error('Failed to load dividends');
      },
    });

    this.portfolioService.getPositions('all').subscribe({
      next: (rows) => this.positions.set(rows),
      error: (err) => this.logger.error('Failed to load positions for dividend form', err, LOG_FILE),
    });
  }

  isStockExpanded(stockId: number): boolean {
    return this.expandedStockIds().has(stockId);
  }

  isYearExpanded(year: number): boolean {
    return this.expandedYears().has(year);
  }

  toggleStock(stockId: number): void {
    this.expandedStockIds.update((set) => {
      const next = new Set(set);
      if (next.has(stockId)) next.delete(stockId);
      else next.add(stockId);
      return next;
    });
  }

  toggleYear(year: number): void {
    this.expandedYears.update((set) => {
      const next = new Set(set);
      if (next.has(year)) next.delete(year);
      else next.add(year);
      return next;
    });
  }

  clearFocus(): void {
    this.focusStockId.set(null);
    void this.router.navigate(['/dividends'], { queryParams: {} });
  }

  openHolding(stockId: number, event?: Event): void {
    event?.stopPropagation();
    void this.router.navigate(['/holdings', stockId]);
  }

  openAddModal(stockId?: number, event?: Event): void {
    event?.stopPropagation();
    const preferred =
      stockId ||
      this.focusStockId() ||
      this.positions()[0]?.stockId ||
      this.overview()?.stocks[0]?.stockId ||
      0;
    const position = this.positions().find((p) => p.stockId === preferred);
    this.editingDividendId.set(null);
    this.formStockId.set(preferred);
    this.dividendForm = {
      stockId: preferred,
      quantity: position?.currentQuantity || position?.lifetimeBoughtQuantity || 1,
      perShareAmount: 0,
      amount: 0,
      dividendDate: new Date().toISOString().substring(0, 10),
      exDate: '',
      recordDate: '',
      notes: '',
    };
    this.dividendAttempted.set(false);
    this.dividendTouched.set({ stock: false, quantity: false, perShare: false, date: false });
    this.showDividendModal.set(true);
  }

  openEditModal(payout: PortfolioDividend, event?: Event): void {
    event?.stopPropagation();
    this.editingDividendId.set(payout.id);
    this.formStockId.set(payout.stockId);
    this.dividendForm = {
      stockId: payout.stockId,
      quantity: payout.quantity,
      perShareAmount: payout.perShareAmount,
      amount: payout.amount,
      dividendDate: toDateInput(payout.dividendDate),
      exDate: toDateInput(payout.exDate),
      recordDate: toDateInput(payout.recordDate),
      notes: payout.notes ?? '',
    };
    this.dividendAttempted.set(false);
    this.dividendTouched.set({ stock: false, quantity: false, perShare: false, date: false });
    this.showDividendModal.set(true);
  }

  closeDividendModal(): void {
    this.showDividendModal.set(false);
    this.editingDividendId.set(null);
    this.dividendAttempted.set(false);
    this.dividendTouched.set({ stock: false, quantity: false, perShare: false, date: false });
  }

  onStockFormChange(): void {
    this.touchDividend('stock');
    this.formStockId.set(Number(this.dividendForm.stockId) || 0);
    if (this.isEditing()) return;
    const position = this.positions().find((p) => p.stockId === Number(this.dividendForm.stockId));
    if (position) {
      this.dividendForm.quantity = position.currentQuantity || position.lifetimeBoughtQuantity || 1;
    }
    this.onDividendCalc();
  }

  onPriceFocus(event: Event): void {
    const el = event.target as HTMLInputElement;
    const n = Number(el.value);
    if (!Number.isFinite(n) || n === 0) el.value = '';
  }

  onPerShareInput(event: Event): void {
    const el = event.target as HTMLInputElement;
    const raw = el.value.trim();
    if (raw === '' || raw === '.') return;
    const n = Number(raw);
    if (!Number.isFinite(n) || n < 0) return;
    this.dividendForm.perShareAmount = n;
    this.onDividendCalc();
  }

  onPerShareBlur(event: Event): void {
    const el = event.target as HTMLInputElement;
    let n = Number(el.value);
    if (!Number.isFinite(n) || n < 0) n = 0;
    this.dividendForm.perShareAmount = Number(n.toFixed(2));
    el.value = this.dividendForm.perShareAmount > 0 ? this.dividendForm.perShareAmount.toFixed(2) : '0';
    this.touchDividend('perShare');
    this.onDividendCalc();
  }

  onDividendCalc(): void {
    const qty = Number(this.dividendForm.quantity) || 0;
    const per = Number(this.dividendForm.perShareAmount) || 0;
    this.dividendForm.amount = Number(((qty > 0 ? qty : 0) * (per > 0 ? per : 0)).toFixed(2));
  }

  submitDividend(): void {
    this.dividendAttempted.set(true);
    const firstError =
      this.dividendFieldError('stock') ||
      this.dividendFieldError('quantity') ||
      this.dividendFieldError('perShare') ||
      this.dividendFieldError('date');
    if (firstError) {
      this.toast.warning(firstError);
      return;
    }

    const payload: AddDividendRequest | UpdateDividendRequest = {
      stockId: Number(this.dividendForm.stockId),
      quantity: this.dividendForm.quantity,
      perShareAmount: this.dividendForm.perShareAmount,
      amount: this.dividendForm.amount || undefined,
      dividendDate: this.dividendForm.dividendDate,
      exDate: this.dividendForm.exDate || undefined,
      recordDate: this.dividendForm.recordDate || undefined,
      notes: this.dividendForm.notes || undefined,
    };

    const editId = this.editingDividendId();
    this.savingDividend.set(true);
    const request$ =
      editId === null
        ? this.portfolioService.addDividend(payload)
        : this.portfolioService.updateDividend(editId, payload);

    request$.subscribe({
      next: (res) => {
        this.savingDividend.set(false);
        if (res?.success) {
          this.toast.success(editId === null ? 'Dividend recorded' : 'Dividend updated');
          this.closeDividendModal();
          this.expandedStockIds.update((set) => new Set(set).add(payload.stockId));
          this.load();
        } else {
          this.toast.error(apiErrorMessage(res, 'Could not save dividend'));
        }
      },
      error: (err) => {
        this.savingDividend.set(false);
        this.logger.error('Save dividend failed', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Could not save dividend'));
      },
    });
  }

  async deleteDividend(id: number, event?: Event): Promise<void> {
    event?.stopPropagation();
    const ok = await this.confirm.open({
      title: 'Delete dividend',
      message: 'Remove this dividend record? This does not change buy or sell lots.',
      confirmText: 'Delete',
      confirmColor: 'danger',
    });
    if (!ok) return;

    this.portfolioService.deleteDividend(id).subscribe({
      next: (res) => {
        if (res?.success) {
          this.toast.success('Dividend deleted');
          this.load();
        } else {
          this.toast.error(apiErrorMessage(res, 'Delete failed'));
        }
      },
      error: (err) => {
        this.logger.error('Delete dividend failed', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Delete failed'));
      },
    });
  }

  openImportModal(): void {
    this.importPreview.set([]);
    this.importErrors.set([]);
    this.importFileName.set('');
    this.showImportModal.set(true);
  }

  closeImportModal(): void {
    this.showImportModal.set(false);
    this.importPreview.set([]);
    this.importErrors.set([]);
    this.importFileName.set('');
  }

  downloadImportSample(): void {
    downloadTextFile('import-dividend-sample.csv', sampleCsv('dividend'));
  }

  onImportFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.importFileName.set(file.name);
    const reader = new FileReader();
    reader.onload = () => {
      const parsed = parseImportCsv(reader.result as string, 'dividend');
      this.importPreview.set(parsed.rows);
      this.importErrors.set(parsed.errors);
      if (parsed.rows.length === 0) {
        this.toast.warning(parsed.errors[0] || 'No valid rows found in CSV');
      }
    };
    reader.readAsText(file);
    input.value = '';
  }

  async submitImport(): Promise<void> {
    const rows = this.importPreview();
    if (rows.length === 0) return;
    this.importSaving.set(true);
    let successCount = 0;
    let failCount = 0;

    for (const row of rows) {
      try {
        const searchRes = await firstValueFrom(
          this.stockService.search({
            pageNumber: 1,
            pageSize: 8,
            globalSearch: row.symbol,
            filters: [],
            sorts: [],
          }),
        );
        const stock = searchRes?.data?.find((s) => s.symbol.toLowerCase() === row.symbol.toLowerCase());
        const stockId = stock ? (stock.id || stock.stockId) : 0;
        if (!stockId) {
          failCount++;
          continue;
        }

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
      } catch {
        failCount++;
      }
    }

    this.importSaving.set(false);
    this.closeImportModal();
    this.load();
    if (successCount > 0) this.toast.success(`Imported ${successCount} dividend${successCount === 1 ? '' : 's'}`);
    if (failCount > 0) this.toast.warning(`${failCount} row(s) failed (unknown symbol or no buy lot)`);
  }
}
