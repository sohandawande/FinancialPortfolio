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
import { ActivatedRoute, Router } from '@angular/router';

import { PageHeader } from '../../../layout/components/page-header/page-header';
import { StockLogo } from '../../../layout/components/stock-logo/stock-logo';
import { FpModal } from '../../../layout/components/fp-modal/fp-modal';
import { PageHeaderAction } from '../../../core/models/page-header/page-header-action.model';
import { PortfolioService } from '../../../core/services/portfolio/portfolio.service';
import { ToastService } from '../../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../../core/services/confirm-modal/confirm-modal.service';
import { ClientLoggerService } from '../../../core/services/logs/client-logger.service';
import { PortfolioPositionDetail } from '../../../core/models/portfolio/portfolio-position-detail.model';
import { PortfolioHolding } from '../../../core/models/portfolio/portfolio-holding.model';
import { PortfolioSold } from '../../../core/models/portfolio/portfolio-sold.model';
import { UpdateHoldRequest } from '../../../core/models/portfolio/update-hold-request.model';
import { UpdateSoldRequest } from '../../../core/models/portfolio/update-sold-request.model';
import { AddDividendRequest } from '../../../core/models/portfolio/add-dividend-request.model';
import { PortfolioDividend } from '../../../core/models/portfolio/portfolio-dividend.model';
import { PortfolioDividendYearTotal } from '../../../core/models/portfolio/portfolio-dividend-year-total.model';
import { isFutureIsoDate } from '../../../core/helper/validators/app.validators';
import { apiErrorMessage } from '../../../core/helper/validators/api-error.helper';

const LOG_FILE = 'holding-details.ts';

@Component({
  selector: 'app-holding-details',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeader, StockLogo, FpModal, CurrencyPipe, DecimalPipe, DatePipe],
  templateUrl: './holding-details.html',
  styleUrl: './holding-details.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HoldingDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly portfolioService = inject(PortfolioService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly logger = inject(ClientLoggerService);

  readonly detail = signal<PortfolioPositionDetail | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly logoBroken = signal(false);

  readonly showDividendModal = signal(false);
  readonly savingDividend = signal(false);
  readonly selectedDividendYear = signal<number | 'all'>('all');

  readonly editingDividendId = signal<number | null>(null);

  readonly showHoldModal = signal(false);
  readonly savingHold = signal(false);
  readonly editingHold = signal<PortfolioHolding | null>(null);
  readonly holdSoldQuantity = signal(0);
  readonly holdAttempted = signal(false);
  readonly holdTouched = signal({ quantity: false, price: false, date: false });
  readonly soldAttempted = signal(false);
  readonly soldTouched = signal({ quantity: false, price: false, date: false });
  readonly dividendAttempted = signal(false);
  readonly dividendTouched = signal({ quantity: false, amount: false, date: false });

  holdFieldError(field: 'quantity' | 'price' | 'date'): string {
    if (!(this.holdAttempted() || this.holdTouched()[field])) return '';
    const min = Math.max(1, this.holdSoldQuantity());
    if (field === 'quantity' && this.holdForm.quantity < min) {
      return this.holdSoldQuantity() > 0
        ? `Quantity cannot be below ${this.holdSoldQuantity()} already sold`
        : 'Quantity must be at least 1';
    }
    if (field === 'price' && this.holdForm.purchasePrice <= 0) return 'Enter a purchase price greater than 0';
    if (field === 'date') {
      if (!this.holdForm.purchaseDate) return 'Purchase date is required';
      if (isFutureIsoDate(this.holdForm.purchaseDate)) return 'Purchase date cannot be in the future';
    }
    return '';
  }

  soldFieldError(field: 'quantity' | 'price' | 'date'): string {
    if (!(this.soldAttempted() || this.soldTouched()[field])) return '';
    if (field === 'quantity') {
      if (this.soldForm.sellQuantity < 1) return 'Sell quantity must be at least 1';
      if (this.soldForm.sellQuantity > this.soldMaxQty()) return `Quantity cannot exceed ${this.soldMaxQty()}`;
    }
    if (field === 'price' && this.soldForm.sellPrice <= 0) return 'Enter a sell price greater than 0';
    if (field === 'date') {
      if (!this.soldForm.soldDate) return 'Sold date is required';
      if (isFutureIsoDate(this.soldForm.soldDate)) return 'Sold date cannot be in the future';
    }
    return '';
  }

  dividendFieldError(field: 'quantity' | 'amount' | 'date'): string {
    if (!(this.dividendAttempted() || this.dividendTouched()[field])) return '';
    if (field === 'quantity' && this.dividendForm.quantity < 1) return 'Quantity must be at least 1';
    if (field === 'amount' && this.dividendForm.perShareAmount <= 0 && this.dividendForm.amount <= 0) {
      return 'Enter per-share or total dividend amount';
    }
    if (field === 'date') {
      if (!this.dividendForm.dividendDate) return 'Credit date is required';
      if (isFutureIsoDate(this.dividendForm.dividendDate)) return 'Credit date cannot be in the future';
    }
    return '';
  }

  holdForm = {
    quantity: 1,
    purchasePrice: 0,
    purchaseDate: new Date().toISOString().substring(0, 10),
    exchange: 1,
    notes: '',
  };

  readonly showSoldModal = signal(false);
  readonly savingSold = signal(false);
  readonly editingSold = signal<PortfolioSold | null>(null);
  readonly soldMaxQty = signal(0);

  soldForm = {
    sellQuantity: 1,
    sellPrice: 0,
    soldDate: new Date().toISOString().substring(0, 10),
    notes: '',
  };

  dividendForm = {
    quantity: 1,
    perShareAmount: 0,
    amount: 0,
    dividendDate: new Date().toISOString().substring(0, 10),
    exDate: '',
    recordDate: '',
    notes: '',
  };

  readonly isEditingDividend = computed(() => this.editingDividendId() !== null);

  readonly position = computed(() => this.detail()?.position ?? null);

  readonly initials = computed(() => {
    const p = this.position();
    return (p?.symbol || '?').substring(0, 2).toUpperCase();
  });

  readonly dividendsByYear = computed<PortfolioDividendYearTotal[]>(() => {
    const detail = this.detail();
    if (!detail) return [];
    if (detail.dividendsByYear?.length) {
      return [...detail.dividendsByYear].sort((a, b) => b.year - a.year);
    }

    const map = new Map<number, PortfolioDividendYearTotal>();
    for (const row of detail.dividends ?? []) {
      const year = new Date(row.dividendDate).getFullYear();
      const current = map.get(year) ?? { year, amount: 0, count: 0 };
      current.amount += row.amount;
      current.count += 1;
      map.set(year, current);
    }

    return [...map.values()]
      .sort((a, b) => b.year - a.year)
      .map((x) => ({ ...x, amount: Number(x.amount.toFixed(2)) }));
  });

  readonly selectedYearTotal = computed<PortfolioDividendYearTotal | null>(() => {
    const year = this.selectedDividendYear();
    if (year === 'all') return null;
    return (
      this.dividendsByYear().find((y) => y.year === year) ?? {
        year,
        amount: 0,
        count: 0,
      }
    );
  });

  readonly visibleDividends = computed<PortfolioDividend[]>(() => {
    const rows = this.detail()?.dividends ?? [];
    const year = this.selectedDividendYear();
    if (year === 'all') return rows;
    return rows.filter((row) => new Date(row.dividendDate).getFullYear() === year);
  });

  readonly headerActions = computed<PageHeaderAction[]>(() => [
    {
      id: 'back',
      label: 'Back to holdings',
      icon: 'bi-arrow-left',
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
      id: 'all-dividends',
      label: 'All dividends',
      icon: 'bi-list-ul',
      color: 'outline-primary',
      disabled: this.loading() || !this.position(),
    },
    {
      id: 'dividend',
      label: 'Add dividend',
      icon: 'bi-cash-coin',
      color: 'primary',
      disabled: this.loading() || !this.position(),
    },
  ]);

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('stockId'));
      if (!id) {
        this.notFound.set(true);
        this.loading.set(false);
        return;
      }
      this.load(id);
    });
  }

  onHeaderAction(actionId: string): void {
    if (actionId === 'back') this.back();
    if (actionId === 'refresh') {
      const id = this.position()?.stockId;
      if (id) this.load(id);
    }
    if (actionId === 'all-dividends') this.openAllDividends();
    if (actionId === 'dividend') this.openDividendModal();
  }

  back(): void {
    void this.router.navigate(['/holdings']);
  }

  load(stockId: number): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.logoBroken.set(false);
    this.portfolioService.getPositionDetail(stockId).subscribe({
      next: (data) => {
        this.detail.set(data);
        this.notFound.set(!data);
        this.loading.set(false);
      },
      error: (err) => {
        this.logger.error('Failed to load position detail', err, LOG_FILE);
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  onYearChange(year: number | 'all' | string): void {
    this.selectedDividendYear.set(year === 'all' ? 'all' : Number(year));
  }

  onLogoError(): void {
    this.logoBroken.set(true);
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

  lotBadge(status: number): { text: string; class: string } {
    switch (status) {
      case 1: return { text: 'Open', class: 'bg-success' };
      case 2: return { text: 'Partial', class: 'bg-primary' };
      case 3: return { text: 'Sold', class: 'bg-secondary' };
      default: return { text: 'Unknown', class: 'bg-dark' };
    }
  }

  eventIcon(type: string): string {
    if (type === 'Buy') return 'bi-plus-circle text-success';
    if (type === 'Sell') return 'bi-dash-circle text-danger';
    return 'bi-cash-coin text-warning';
  }

  openAllDividends(): void {
    const id = this.position()?.stockId;
    if (!id) return;
    void this.router.navigate(['/dividends'], { queryParams: { stockId: id } });
  }

  openDividendModal(): void {
    const p = this.position();
    this.editingDividendId.set(null);
    this.dividendForm = {
      quantity: p?.currentQuantity || p?.lifetimeBoughtQuantity || 1,
      perShareAmount: 0,
      amount: 0,
      dividendDate: new Date().toISOString().substring(0, 10),
      exDate: '',
      recordDate: '',
      notes: '',
    };
    this.dividendAttempted.set(false);
    this.dividendTouched.set({ quantity: false, amount: false, date: false });
    this.showDividendModal.set(true);
  }

  openEditDividend(row: PortfolioDividend): void {
    this.editingDividendId.set(row.id);
    this.dividendForm = {
      quantity: row.quantity,
      perShareAmount: row.perShareAmount,
      amount: row.amount,
      dividendDate: (row.dividendDate || '').substring(0, 10),
      exDate: (row.exDate || '').substring(0, 10),
      recordDate: (row.recordDate || '').substring(0, 10),
      notes: row.notes ?? '',
    };
    this.dividendAttempted.set(false);
    this.dividendTouched.set({ quantity: false, amount: false, date: false });
    this.showDividendModal.set(true);
  }

  closeDividendModal(): void {
    this.showDividendModal.set(false);
    this.editingDividendId.set(null);
  }

  onDividendCalc(): void {
    const qty = Number(this.dividendForm.quantity) || 0;
    const per = Number(this.dividendForm.perShareAmount) || 0;
    if (qty > 0 && per > 0) {
      this.dividendForm.amount = Number((qty * per).toFixed(2));
    }
  }

  submitDividend(): void {
    const p = this.position();
    if (!p) return;
    this.dividendAttempted.set(true);
    const firstError =
      this.dividendFieldError('quantity') ||
      this.dividendFieldError('amount') ||
      this.dividendFieldError('date');
    if (firstError) {
      this.toast.warning(firstError);
      return;
    }
    this.savingDividend.set(true);
    const request: AddDividendRequest = {
      stockId: p.stockId,
      quantity: this.dividendForm.quantity,
      perShareAmount: this.dividendForm.perShareAmount,
      amount: this.dividendForm.amount || undefined,
      dividendDate: this.dividendForm.dividendDate,
      exDate: this.dividendForm.exDate || undefined,
      recordDate: this.dividendForm.recordDate || undefined,
      notes: this.dividendForm.notes || undefined,
    };
    const editId = this.editingDividendId();
    const request$ =
      editId === null
        ? this.portfolioService.addDividend(request)
        : this.portfolioService.updateDividend(editId, request);
    request$.subscribe({
      next: (res) => {
        this.savingDividend.set(false);
        if (res?.success) {
          this.toast.success(editId === null ? 'Dividend recorded' : 'Dividend updated');
          this.closeDividendModal();
          this.load(p.stockId);
        } else {
          this.toast.error(apiErrorMessage(res, 'Could not save dividend'));
        }
      },
      error: (err) => {
        this.savingDividend.set(false);
        this.logger.error('Add dividend failed', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Could not save dividend'));
      },
    });
  }

  openEditHold(hold: PortfolioHolding): void {
    this.editingHold.set(hold);
    this.holdSoldQuantity.set(Math.max(0, hold.quantity - hold.remainingQuantity));
    this.holdForm = {
      quantity: hold.quantity,
      purchasePrice: hold.purchasePrice,
      purchaseDate: (hold.purchaseDate || '').substring(0, 10),
      exchange: hold.exchange,
      notes: hold.holdNotes ?? '',
    };
    this.holdAttempted.set(false);
    this.holdTouched.set({ quantity: false, price: false, date: false });
    this.showHoldModal.set(true);
  }

  closeHoldModal(): void {
    this.showHoldModal.set(false);
    this.editingHold.set(null);
  }

  submitHold(): void {
    const hold = this.editingHold();
    const stockId = this.position()?.stockId;
    if (!hold || !stockId) return;
    this.holdAttempted.set(true);
    const firstError =
      this.holdFieldError('quantity') ||
      this.holdFieldError('price') ||
      this.holdFieldError('date');
    if (firstError) {
      this.toast.warning(firstError);
      return;
    }
    this.savingHold.set(true);
    const request: UpdateHoldRequest = {
      quantity: this.holdForm.quantity,
      purchasePrice: this.holdForm.purchasePrice,
      purchaseDate: this.holdForm.purchaseDate,
      exchange: this.holdForm.exchange,
      notes: this.holdForm.notes || undefined,
    };
    this.portfolioService.updateHold(hold.id, request).subscribe({
      next: (res) => {
        this.savingHold.set(false);
        if (res?.success) {
          this.toast.success('Buy lot updated');
          this.closeHoldModal();
          this.load(stockId);
        } else {
          this.toast.error(apiErrorMessage(res, 'Update failed'));
        }
      },
      error: (err) => {
        this.savingHold.set(false);
        this.logger.error('Update hold failed', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Update failed'));
      },
    });
  }

  async deleteHold(hold: PortfolioHolding): Promise<void> {
    const soldQty = Math.max(0, hold.quantity - hold.remainingQuantity);
    const ok = await this.confirm.open({
      title: 'Delete buy lot',
      message: soldQty > 0
        ? `Delete this ${hold.quantity}-share lot? Related ${soldQty} sold share(s) will also be removed.`
        : `Delete this ${hold.quantity}-share lot? This cannot be undone.`,
      confirmText: 'Delete',
      confirmColor: 'danger',
    });
    if (!ok) return;
    const stockId = this.position()?.stockId;
    this.portfolioService.deleteHold(hold.id).subscribe({
      next: (res) => {
        if (res?.success) {
          this.toast.success(res.message || 'Buy lot deleted');
          if (stockId) this.load(stockId);
        } else {
          this.toast.error(apiErrorMessage(res, 'Delete failed'));
        }
      },
      error: (err) => {
        this.logger.error('Delete hold failed', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Delete failed'));
      },
    });
  }

  openEditSold(sold: PortfolioSold): void {
    const hold = this.detail()?.buys.find((b) => b.id === sold.holdId);
    const max = sold.sellQuantity + (hold?.remainingQuantity ?? 0);
    this.editingSold.set(sold);
    this.soldMaxQty.set(Math.max(1, max));
    this.soldForm = {
      sellQuantity: sold.sellQuantity,
      sellPrice: sold.sellPrice,
      soldDate: (sold.soldDate || '').substring(0, 10),
      notes: sold.soldNotes ?? '',
    };
    this.soldAttempted.set(false);
    this.soldTouched.set({ quantity: false, price: false, date: false });
    this.showSoldModal.set(true);
  }

  closeSoldModal(): void {
    this.showSoldModal.set(false);
    this.editingSold.set(null);
  }

  submitSold(): void {
    const sold = this.editingSold();
    const stockId = this.position()?.stockId;
    if (!sold || !stockId) return;
    this.soldAttempted.set(true);
    const firstError =
      this.soldFieldError('quantity') ||
      this.soldFieldError('price') ||
      this.soldFieldError('date');
    if (firstError) {
      this.toast.warning(firstError);
      return;
    }
    this.savingSold.set(true);
    const request: UpdateSoldRequest = {
      sellQuantity: this.soldForm.sellQuantity,
      sellPrice: this.soldForm.sellPrice,
      soldDate: this.soldForm.soldDate,
      notes: this.soldForm.notes || undefined,
    };
    this.portfolioService.updateSold(sold.id, request).subscribe({
      next: (res) => {
        this.savingSold.set(false);
        if (res?.success) {
          this.toast.success('Sell record updated');
          this.closeSoldModal();
          this.load(stockId);
        } else {
          this.toast.error(apiErrorMessage(res, 'Update failed'));
        }
      },
      error: (err) => {
        this.savingSold.set(false);
        this.logger.error('Update sold failed', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Update failed'));
      },
    });
  }

  async deleteSold(sold: PortfolioSold): Promise<void> {
    const ok = await this.confirm.open({
      title: 'Delete sell',
      message: `Remove the sale of ${sold.sellQuantity} shares? Quantity is restored to the buy lot.`,
      confirmText: 'Delete',
      confirmColor: 'danger',
    });
    if (!ok) return;
    const stockId = this.position()?.stockId;
    this.portfolioService.deleteSold(sold.id).subscribe({
      next: (res) => {
        if (res?.success) {
          this.toast.success(res.message || 'Sell deleted');
          if (stockId) this.load(stockId);
        } else {
          this.toast.error(apiErrorMessage(res, 'Delete failed'));
        }
      },
      error: (err) => {
        this.logger.error('Delete sold failed', err, LOG_FILE);
        this.toast.error(apiErrorMessage(err, 'Delete failed'));
      },
    });
  }

  async deleteDividend(id: number): Promise<void> {
    const ok = await this.confirm.open({
      title: 'Delete dividend',
      message: 'Remove this dividend record? This does not change buy or sell lots.',
      confirmText: 'Delete',
      confirmColor: 'danger',
    });
    if (!ok) return;
    const stockId = this.position()?.stockId;
    this.portfolioService.deleteDividend(id).subscribe({
      next: (res) => {
        if (res?.success) {
          this.toast.success('Dividend deleted');
          if (stockId) this.load(stockId);
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
}