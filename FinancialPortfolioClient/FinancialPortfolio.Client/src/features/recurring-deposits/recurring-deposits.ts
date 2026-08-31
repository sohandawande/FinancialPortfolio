import {
  Component,
  OnInit,
  inject,
  signal,
  ChangeDetectionStrategy,
  DestroyRef,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';

import { PageHeader } from '../../layout/components/page-header/page-header';
import { FpModal } from '../../layout/components/fp-modal/fp-modal';
import { FpDate } from '../../layout/components/fp-date/fp-date';
import { FpDropdownSelect, FpDropdownSelectOption } from '../../layout/components/fp-dropdown-select/fp-dropdown-select';
import { PageHeaderAction } from '../../core/models/page-header/page-header-action.model';
import { WealthService } from '../../core/services/wealth/wealth.service';
import { BankService } from '../../core/services/bank/bank.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../core/services/confirm-modal/confirm-modal.service';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import {
  RecurringDeposit,
  RdInstallment,
  STATUS_LABELS,
  RD_INSTALLMENT_STATUS_LABELS,
  RD_PAYMENT_MODE_LABELS,
  UpsertRecurringDepositRequest,
  PayRdInstallmentRequest,
  BankSuggestion,
} from '../../core/models/wealth/wealth.models';

@Component({
  selector: 'app-recurring-deposits',
  standalone: true,
  imports: [FpDropdownSelect, CommonModule, FormsModule, PageHeader, FpModal, FpDate, CurrencyPipe, DatePipe],
  templateUrl: './recurring-deposits.html',
  styleUrls: ['../wealth/wealth-list.css', './recurring-deposits.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecurringDeposits implements OnInit {
  private readonly api = inject(WealthService);
  private readonly banks = inject(BankService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly destroyRef = inject(DestroyRef);

  readonly rows = signal<RecurringDeposit[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly showModal = signal(false);
  readonly showPayModal = signal(false);
  readonly editId = signal<number | null>(null);
  readonly expandedId = signal<number | null>(null);
  readonly payingId = signal<number | null>(null);
  readonly installments = signal<RdInstallment[]>([]);
  readonly loadingInstallments = signal(false);
  readonly bankSuggestions = signal<BankSuggestion[]>([]);
  readonly showBankDrop = signal(false);

  readonly statusOptions: FpDropdownSelectOption[] = [
    { value: 1, label: 'Active' },
    { value: 2, label: 'Matured' },
    { value: 3, label: 'Closed' },
  ];

  readonly paymentModeOptions: FpDropdownSelectOption[] = [
    { value: 1, label: 'Auto debit' },
    { value: 2, label: 'NEFT' },
    { value: 3, label: 'RTGS' },
    { value: 4, label: 'IMPS' },
    { value: 5, label: 'UPI' },
    { value: 6, label: 'Cash' },
    { value: 7, label: 'Cheque' },
    { value: 8, label: 'Other' },
  ];

  readonly statusLabels = STATUS_LABELS;
  readonly installmentStatusLabels = RD_INSTALLMENT_STATUS_LABELS;
  readonly paymentModeLabels = RD_PAYMENT_MODE_LABELS;

  form: UpsertRecurringDepositRequest = this.empty();
  payForm: PayRdInstallmentRequest = this.emptyPay();
  readonly payTarget = signal<RecurringDeposit | null>(null);

  private readonly bankSearch$ = new Subject<string>();

  readonly headerActions: PageHeaderAction[] = [
    { id: 'refresh', label: 'Refresh', icon: 'bi-arrow-clockwise', color: 'outline-secondary' },
    { id: 'add', label: 'Add RD', icon: 'bi-plus-lg', color: 'primary' },
  ];

  ngOnInit(): void {
    this.load();

    this.bankSearch$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap((q) => (q.trim().length < 1 ? of([]) : this.banks.search(q))),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (list) => {
          this.bankSuggestions.set(list);
          this.showBankDrop.set(list.length > 0);
        },
        error: () => {
          this.bankSuggestions.set([]);
          this.showBankDrop.set(false);
        },
      });
  }

  onHeaderAction(id: string): void {
    if (id === 'refresh') this.load();
    if (id === 'add') this.open();
  }

  toggle(row: RecurringDeposit): void {
    const next = this.expandedId() === row.id ? null : row.id;
    this.expandedId.set(next);
    if (next != null) this.loadInstallments(next);
    else this.installments.set([]);
  }

  isOpen(row: RecurringDeposit): boolean {
    return this.expandedId() === row.id;
  }

  progressPct(row: RecurringDeposit): number {
    if (!row.tenureMonths) return 0;
    return Math.min(100, Math.round((row.installmentsPaid / row.tenureMonths) * 100));
  }

  canPay(row: RecurringDeposit): boolean {
    return row.status === 1 && row.installmentsPaid < row.tenureMonths;
  }

  open(row?: RecurringDeposit): void {
    this.editId.set(row?.id ?? null);
    this.form = row
      ? {
          bankName: row.bankName,
          bankIfsc: row.bankIfsc ?? '',
          accountRef: row.accountRef ?? '',
          linkedAccountNumber: row.linkedAccountNumber ?? '',
          linkedIfsc: row.linkedIfsc ?? '',
          monthlyAmount: row.monthlyAmount,
          interestRate: row.interestRate,
          tenureMonths: row.tenureMonths,
          installmentsPaid: row.installmentsPaid,
          startDate: row.startDate.substring(0, 10),
          notes: row.notes ?? '',
          status: row.status,
        }
      : this.empty();
    this.showBankDrop.set(false);
    this.showModal.set(true);
  }

  onBankNameInput(value: string): void {
    this.form.bankName = value;
    this.bankSearch$.next(value);
  }

  onBankFocus(): void {
    this.bankSearch$.next(this.form.bankName || '');
  }

  selectBank(s: BankSuggestion): void {
    this.form.bankName = s.name;
    this.showBankDrop.set(false);
    this.bankSuggestions.set([]);
  }

  onIfscBlur(): void {
    const code = (this.form.bankIfsc || '').trim().toUpperCase();
    this.form.bankIfsc = code;
    if (code.length !== 11) return;

    this.banks.lookupIfsc(code).subscribe({
      next: (info) => {
        if (!info) {
          this.toast.warning('IFSC not found');
          return;
        }
        if (!this.form.bankName?.trim()) this.form.bankName = info.bank;
        this.toast.success(`${info.bank}${info.branch ? ' · ' + info.branch : ''}`);
      },
      error: () => this.toast.warning('Could not resolve IFSC'),
    });
  }

  onStatusChange(value: number | string | null): void {
    const n = Number(value);
    if (n >= 1 && n <= 3) this.form.status = n as 1 | 2 | 3;
  }

  onPayModeChange(value: number | string | null): void {
    const n = Number(value);
    if (n >= 1 && n <= 8) this.payForm.paymentMode = n as 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8;
  }

  openPay(row: RecurringDeposit, event?: Event): void {
    event?.stopPropagation();
    if (!this.canPay(row)) {
      this.toast.info('All installments already paid or RD is not active');
      return;
    }
    this.payTarget.set(row);
    this.payForm = {
      installmentNumber: row.installmentsPaid + 1,
      paidDate: new Date().toISOString().substring(0, 10),
      amount: row.monthlyAmount,
      fromBankName: row.bankName,
      fromAccountNumber: row.linkedAccountNumber ?? '',
      fromIfsc: row.linkedIfsc ?? row.bankIfsc ?? '',
      transactionReference: '',
      paymentMode: 1,
      penaltyAmount: null,
      notes: '',
    };
    this.showPayModal.set(true);
  }

  confirmPay(): void {
    const row = this.payTarget();
    if (!row) return;

    this.payingId.set(row.id);
    this.api.payRdInstallment(row.id, this.payForm).subscribe({
      next: (res) => {
        this.payingId.set(null);
        if (!res.success) {
          this.toast.error(res.message);
          return;
        }
        this.toast.success(res.message || 'Installment recorded');
        this.showPayModal.set(false);
        this.expandedId.set(row.id);
        this.load();
        this.loadInstallments(row.id);
      },
      error: (err) => {
        this.payingId.set(null);
        this.toast.error(apiErrorMessage(err, 'Could not record installment'));
      },
    });
  }

  async removeInstallment(row: RecurringDeposit, inst: RdInstallment, event?: Event): Promise<void> {
    event?.stopPropagation();
    const ok = await this.confirm.open({
      title: 'Delete installment',
      message: `Remove installment #${inst.installmentNumber}?`,
      confirmText: 'Delete',
    });
    if (!ok) return;

    this.api.deleteRdInstallment(row.id, inst.id).subscribe({
      next: () => {
        this.toast.success('Installment deleted');
        this.load();
        this.loadInstallments(row.id);
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'Delete failed')),
    });
  }

  save(): void {
    if (!this.form.bankName?.trim()) {
      this.toast.warning('Bank name is required');
      return;
    }
    if (!(this.form.monthlyAmount > 0)) {
      this.toast.warning('Monthly installment must be greater than 0');
      return;
    }
    if (!(this.form.tenureMonths >= 1)) {
      this.toast.warning('Tenure must be at least 1 month');
      return;
    }
    if (this.form.installmentsPaid < 0 || this.form.installmentsPaid > this.form.tenureMonths) {
      this.toast.warning('Installments paid must be between 0 and tenure');
      return;
    }

    this.saving.set(true);
    const req$ = this.editId()
      ? this.api.updateRecurringDeposit(this.editId()!, this.form)
      : this.api.addRecurringDeposit(this.form);

    req$.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (!res.success) {
          this.toast.error(res.message);
          return;
        }
        this.toast.success(res.message || 'Saved');
        this.showModal.set(false);
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.toast.error(apiErrorMessage(err, 'Save failed'));
      },
    });
  }

  async remove(row: RecurringDeposit, event?: Event): Promise<void> {
    event?.stopPropagation();
    const ok = await this.confirm.open({
      title: 'Delete RD',
      message: `Remove ${row.bankName} RD permanently? All installment history will be deleted.`,
      confirmText: 'Delete',
    });
    if (!ok) return;

    this.api.deleteRecurringDeposit(row.id).subscribe({
      next: () => {
        this.toast.success('Deleted');
        if (this.expandedId() === row.id) this.expandedId.set(null);
        this.load();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'Delete failed')),
    });
  }

  load(): void {
    this.loading.set(true);
    this.api.recurringDeposits().subscribe({
      next: (rows) => {
        this.rows.set(rows ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(apiErrorMessage(err, 'Failed to load RDs'));
      },
    });
  }

  private loadInstallments(rdId: number): void {
    this.loadingInstallments.set(true);
    this.api.getRdInstallments(rdId).subscribe({
      next: (rows) => {
        this.installments.set(rows ?? []);
        this.loadingInstallments.set(false);
      },
      error: () => {
        this.installments.set([]);
        this.loadingInstallments.set(false);
      },
    });
  }

  private empty(): UpsertRecurringDepositRequest {
    return {
      bankName: '',
      bankIfsc: '',
      accountRef: '',
      linkedAccountNumber: '',
      linkedIfsc: '',
      monthlyAmount: 0,
      interestRate: 0,
      tenureMonths: 12,
      installmentsPaid: 0,
      startDate: new Date().toISOString().substring(0, 10),
      notes: '',
      status: 1,
    };
  }

  private emptyPay(): PayRdInstallmentRequest {
    return {
      installmentNumber: 1,
      paidDate: new Date().toISOString().substring(0, 10),
      amount: 0,
      fromBankName: '',
      fromAccountNumber: '',
      fromIfsc: '',
      transactionReference: '',
      paymentMode: 1,
      penaltyAmount: null,
      notes: '',
    };
  }
}
