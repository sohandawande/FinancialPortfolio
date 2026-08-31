import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { PageHeader } from '../../layout/components/page-header/page-header';
import { FpModal } from '../../layout/components/fp-modal/fp-modal';
import { FpDate } from '../../layout/components/fp-date/fp-date';
import { FpDropdownSelect, FpDropdownSelectOption } from '../../layout/components/fp-dropdown-select/fp-dropdown-select';
import { PageHeaderAction } from '../../core/models/page-header/page-header-action.model';
import { WealthService } from '../../core/services/wealth/wealth.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../core/services/confirm-modal/confirm-modal.service';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import { RecurringDeposit, STATUS_LABELS, UpsertRecurringDepositRequest } from '../../core/models/wealth/wealth.models';

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
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);

  readonly rows = signal<RecurringDeposit[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly showModal = signal(false);
  readonly editId = signal<number | null>(null);
  readonly expandedId = signal<number | null>(null);
  readonly payingId = signal<number | null>(null);

  readonly statusOptions: FpDropdownSelectOption[] = [
    { value: 1, label: 'Active' },
    { value: 2, label: 'Matured' },
    { value: 3, label: 'Closed' },
  ];

  readonly statusLabels = STATUS_LABELS;

  form: UpsertRecurringDepositRequest = this.empty();

  readonly headerActions: PageHeaderAction[] = [
    { id: 'refresh', label: 'Refresh', icon: 'bi-arrow-clockwise', color: 'outline-secondary' },
    { id: 'add', label: 'Add RD', icon: 'bi-plus-lg', color: 'primary' },
  ];

  ngOnInit(): void {
    this.load();
  }

  onHeaderAction(id: string): void {
    if (id === 'refresh') this.load();
    if (id === 'add') this.open();
  }

  toggle(row: RecurringDeposit): void {
    this.expandedId.set(this.expandedId() === row.id ? null : row.id);
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
          accountRef: row.accountRef ?? '',
          monthlyAmount: row.monthlyAmount,
          interestRate: row.interestRate,
          tenureMonths: row.tenureMonths,
          installmentsPaid: row.installmentsPaid,
          startDate: row.startDate.substring(0, 10),
          notes: row.notes ?? '',
          status: row.status,
        }
      : this.empty();
    this.showModal.set(true);
  }

  onStatusChange(value: number | string | null): void {
    const n = Number(value);
    if (n >= 1 && n <= 3) this.form.status = n as 1 | 2 | 3;
  }

  /** Pay next installment without re-entering bank/rate/tenure. */
  async payInstallment(row: RecurringDeposit, event?: Event): Promise<void> {
    event?.stopPropagation();
    if (!this.canPay(row)) {
      this.toast.info('All installments already paid or RD is not active');
      return;
    }

    const next = row.installmentsPaid + 1;
    const ok = await this.confirm.open({
      title: 'Pay installment',
      message: `Record installment #${next} of ${row.tenureMonths} for ${row.bankName}?\n\nAmount: ₹${row.monthlyAmount.toLocaleString('en-IN')}\nRate stays ${row.interestRate}% (unchanged).`,
      confirmText: 'Record payment',
    });
    if (!ok) return;

    this.payingId.set(row.id);
    const body: UpsertRecurringDepositRequest = {
      bankName: row.bankName,
      accountRef: row.accountRef ?? '',
      monthlyAmount: row.monthlyAmount,
      interestRate: row.interestRate,
      tenureMonths: row.tenureMonths,
      installmentsPaid: next,
      startDate: row.startDate.substring(0, 10),
      notes: row.notes ?? '',
      status: next >= row.tenureMonths ? 2 : row.status,
    };

    this.api.updateRecurringDeposit(row.id, body).subscribe({
      next: (res) => {
        this.payingId.set(null);
        if (!res.success) {
          this.toast.error(res.message);
          return;
        }
        this.toast.success(`Installment ${next}/${row.tenureMonths} recorded`);
        this.expandedId.set(row.id);
        this.load();
      },
      error: (err) => {
        this.payingId.set(null);
        this.toast.error(apiErrorMessage(err, 'Could not record installment'));
      },
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
      message: `Remove ${row.bankName} RD permanently?`,
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

  private empty(): UpsertRecurringDepositRequest {
    return {
      bankName: '',
      accountRef: '',
      monthlyAmount: 0,
      interestRate: 0,
      tenureMonths: 12,
      installmentsPaid: 0,
      startDate: new Date().toISOString().substring(0, 10),
      notes: '',
      status: 1,
    };
  }
}
