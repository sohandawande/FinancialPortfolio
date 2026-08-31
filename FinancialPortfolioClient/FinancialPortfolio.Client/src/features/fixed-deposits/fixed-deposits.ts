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
import { FixedDeposit, STATUS_LABELS, UpsertFixedDepositRequest } from '../../core/models/wealth/wealth.models';

@Component({
  selector: 'app-fixed-deposits',
  standalone: true,
  imports: [FpDropdownSelect, CommonModule, FormsModule, PageHeader, FpModal, FpDate, CurrencyPipe, DatePipe],
  templateUrl: './fixed-deposits.html',
  styleUrl: '../wealth/wealth-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FixedDeposits implements OnInit {
  private readonly api = inject(WealthService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);

  readonly rows = signal<FixedDeposit[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly showModal = signal(false);

  readonly interestTypeOptions: FpDropdownSelectOption[] = [
    { value: 1, label: 'Cumulative' },
    { value: 2, label: 'Non-cumulative' },
  ];
  readonly statusOptions: FpDropdownSelectOption[] = [
    { value: 1, label: 'Active' },
    { value: 2, label: 'Matured' },
    { value: 3, label: 'Closed' },
  ];
  readonly editId = signal<number | null>(null);
  readonly statusLabels = STATUS_LABELS;
  form: UpsertFixedDepositRequest = this.empty();

  readonly headerActions: PageHeaderAction[] = [
    { id: 'refresh', label: 'Refresh', icon: 'bi-arrow-clockwise', color: 'outline-secondary' },
    { id: 'export', label: 'Export', icon: 'bi-download', color: 'outline-secondary' },
    { id: 'add', label: 'Add FD', icon: 'bi-plus-lg', color: 'primary' },
  ];

  ngOnInit(): void { this.load(); }
  onHeaderAction(id: string): void {
    if (id === 'refresh') this.load();
    if (id === 'add') this.open();
    if (id === 'export') {
      const rows = this.rows();
      if (!rows.length) { this.toast.warning('Nothing to export'); return; }
      const header = 'Bank,AccountRef,Principal,Rate,TenureMonths,InterestType,StartDate,Status,Notes';
      const lines = [header, ...rows.map((r) => [r.bankName, r.accountRef ?? '', r.principal, r.interestRate, r.tenureMonths, r.interestType, (r.startDate ?? '').substring(0, 10), r.status, `"${(r.notes ?? '').replaceAll('"', '""')}"`].join(','))];
      const blob = new Blob([lines.join('\n')], { type: 'text/csv' });
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = `fixed-deposits-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
    }
  }

  open(row?: FixedDeposit): void {
    this.editId.set(row?.id ?? null);
    this.form = row
      ? {
          bankName: row.bankName,
          accountRef: row.accountRef ?? '',
          principal: row.principal,
          interestRate: row.interestRate,
          tenureMonths: row.tenureMonths,
          interestType: row.interestType,
          startDate: row.startDate.substring(0, 10),
          notes: row.notes ?? '',
          status: row.status,
        }
      : this.empty();
    this.showModal.set(true);
  }

  onInterestTypeChange(value: number | string | null): void {
    const n = Number(value);
    if (n === 1 || n === 2) this.form.interestType = n as 1 | 2;
  }

  onStatusChange(value: number | string | null): void {
    const n = Number(value);
    if (n >= 1 && n <= 3) this.form.status = n as 1 | 2 | 3;
  }

  save(): void {
    if (!this.form.bankName || this.form.principal <= 0 || this.form.tenureMonths < 1) {
      this.toast.warning('Bank, principal and tenure are required');
      return;
    }
    this.saving.set(true);
    const req$ = this.editId()
      ? this.api.updateFixedDeposit(this.editId()!, this.form)
      : this.api.addFixedDeposit(this.form);
    req$.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (!res.success) { this.toast.error(res.message); return; }
        this.toast.success(res.message);
        this.showModal.set(false);
        this.load();
      },
      error: (err) => { this.saving.set(false); this.toast.error(apiErrorMessage(err, 'Save failed')); },
    });
  }

  async remove(row: FixedDeposit): Promise<void> {
    const ok = await this.confirm.open({ title: 'Delete FD', message: `Remove ${row.bankName} FD?`, confirmText: 'Delete' });
    if (!ok) return;
    this.api.deleteFixedDeposit(row.id).subscribe({
      next: () => { this.toast.success('Deleted'); this.load(); },
      error: (err) => this.toast.error(apiErrorMessage(err, 'Delete failed')),
    });
  }

  load(): void {
    this.loading.set(true);
    this.api.fixedDeposits().subscribe({
      next: (rows) => { this.rows.set(rows); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  private empty(): UpsertFixedDepositRequest {
    return {
      bankName: '',
      accountRef: '',
      principal: 0,
      interestRate: 7,
      tenureMonths: 12,
      interestType: 2,
      startDate: new Date().toISOString().substring(0, 10),
      notes: '',
      status: 1,
    };
  }
}
