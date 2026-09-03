import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { PageHeader } from '../../layout/components/page-header/page-header';
import { FpModal } from '../../layout/components/fp-modal/fp-modal';
import { FpDate } from '../../layout/components/fp-date/fp-date';
import { PageHeaderAction } from '../../core/models/page-header/page-header-action.model';
import { WealthService } from '../../core/services/wealth/wealth.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../core/services/confirm-modal/confirm-modal.service';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import {
  InsurancePolicy,
  POLICY_STATUS_LABELS,
  POLICY_TYPE_LABELS,
  PREMIUM_FREQUENCY_LABELS,
  UpsertInsurancePolicyRequest,
} from '../../core/models/wealth/wealth.models';

@Component({
  selector: 'app-insurance-policies',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeader, FpModal, FpDate, CurrencyPipe, DatePipe],
  templateUrl: './insurance-policies.html',
  styleUrl: '../wealth/wealth.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InsurancePolicies implements OnInit {
  private readonly api = inject(WealthService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);

  readonly rows = signal<InsurancePolicy[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly showModal = signal(false);
  readonly editId = signal<number | null>(null);
  readonly statusLabels = POLICY_STATUS_LABELS;
  readonly typeLabels = POLICY_TYPE_LABELS;
  readonly freqLabels = PREMIUM_FREQUENCY_LABELS;
  form: UpsertInsurancePolicyRequest = this.empty();

  readonly headerActions: PageHeaderAction[] = [
    { id: 'refresh', label: 'Refresh', icon: 'bi-arrow-clockwise', color: 'outline-secondary' },
    { id: 'export', label: 'Export', icon: 'bi-download', color: 'outline-secondary' },
    { id: 'add', label: 'Add policy', icon: 'bi-plus-lg', color: 'primary' },
  ];

  ngOnInit(): void {
    this.load();
  }

  onHeaderAction(id: string): void {
    if (id === 'refresh') this.load();
    if (id === 'add') this.open();
    if (id === 'export') {
      const rows = this.rows();
      if (!rows.length) {
        this.toast.warning('Nothing to export');
        return;
      }
      const header =
        'Insurer,PolicyNumber,Plan,Type,SumAssured,Premium,Frequency,PPTYears,TermYears,StartDate,PremiumsPaid,ExpectedMaturity,Status,Notes';
      const lines = [
        header,
        ...rows.map((r) =>
          [
            r.insurerName,
            r.policyNumber,
            r.planName,
            r.policyType,
            r.sumAssured,
            r.premiumAmount,
            r.premiumFrequency,
            r.premiumPayingTermYears,
            r.policyTermYears,
            (r.startDate ?? '').substring(0, 10),
            r.premiumsPaid,
            r.expectedMaturityAmount ?? '',
            r.status,
            `"${(r.notes ?? '').replaceAll('"', '""')}"`,
          ].join(','),
        ),
      ];
      const blob = new Blob([lines.join('\n')], { type: 'text/csv' });
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = `insurance-policies-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
    }
  }

  open(row?: InsurancePolicy): void {
    this.editId.set(row?.id ?? null);
    this.form = row
      ? {
          insurerName: row.insurerName,
          policyNumber: row.policyNumber,
          planName: row.planName,
          policyType: row.policyType,
          sumAssured: row.sumAssured,
          premiumAmount: row.premiumAmount,
          premiumFrequency: row.premiumFrequency,
          premiumPayingTermYears: row.premiumPayingTermYears,
          policyTermYears: row.policyTermYears,
          startDate: row.startDate.substring(0, 10),
          premiumsPaid: row.premiumsPaid,
          expectedMaturityAmount: row.expectedMaturityAmount ?? null,
          status: row.status,
          notes: row.notes ?? '',
        }
      : this.empty();
    this.showModal.set(true);
  }

  save(): void {
    if (!this.form.insurerName || !this.form.policyNumber || !this.form.planName || this.form.sumAssured <= 0) {
      this.toast.warning('Insurer, policy number, plan and sum assured are required');
      return;
    }
    if (this.form.policyTermYears < 1) {
      this.toast.warning('Policy term must be at least 1 year');
      return;
    }
    this.saving.set(true);
    const req$ = this.editId()
      ? this.api.updateInsurancePolicy(this.editId()!, this.form)
      : this.api.addInsurancePolicy(this.form);
    req$.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (!res.success) {
          this.toast.error(res.message);
          return;
        }
        this.toast.success(res.message);
        this.showModal.set(false);
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.toast.error(apiErrorMessage(err, 'Save failed'));
      },
    });
  }

  async remove(row: InsurancePolicy): Promise<void> {
    const ok = await this.confirm.open({
      title: 'Delete policy',
      message: `Remove ${row.insurerName} – ${row.policyNumber}?`,
      confirmText: 'Delete',
    });
    if (!ok) return;
    this.api.deleteInsurancePolicy(row.id).subscribe({
      next: () => {
        this.toast.success('Deleted');
        this.load();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'Delete failed')),
    });
  }

  load(): void {
    this.loading.set(true);
    this.api.insurancePolicies().subscribe({
      next: (rows) => {
        this.rows.set(rows);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private empty(): UpsertInsurancePolicyRequest {
    return {
      insurerName: '',
      policyNumber: '',
      planName: '',
      policyType: 2,
      sumAssured: 0,
      premiumAmount: 0,
      premiumFrequency: 4,
      premiumPayingTermYears: 15,
      policyTermYears: 20,
      startDate: new Date().toISOString().substring(0, 10),
      premiumsPaid: 0,
      expectedMaturityAmount: null,
      status: 1,
      notes: '',
    };
  }
}
