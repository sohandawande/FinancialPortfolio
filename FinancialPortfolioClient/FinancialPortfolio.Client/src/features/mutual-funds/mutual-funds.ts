import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';

import { PageHeader } from '../../layout/components/page-header/page-header';
import { FpModal } from '../../layout/components/fp-modal/fp-modal';
import { FpDate } from '../../layout/components/fp-date/fp-date';
import { PageHeaderAction } from '../../core/models/page-header/page-header-action.model';
import { WealthService } from '../../core/services/wealth/wealth.service';
import { ToastService } from '../../core/services/toast/toast.service';
import { ConfirmModalService } from '../../core/services/confirm-modal/confirm-modal.service';
import { apiErrorMessage } from '../../core/helper/validators/api-error.helper';
import {
  MutualFund,
  MutualFundSchemeLookup,
  SCHEME_LABELS,
  UpsertMutualFundRequest,
} from '../../core/models/wealth/wealth.models';
import {
  csvHeaders,
  csvLines,
  downloadTextFile,
  headerIndex,
  parseCsvLine,
} from '../../core/utils/csv.util';

@Component({
  selector: 'app-mutual-funds',
  standalone: true,
  imports: [CommonModule, FormsModule, PageHeader, FpModal, FpDate, CurrencyPipe, DatePipe],
  templateUrl: './mutual-funds.html',
  styleUrl: '../wealth/wealth.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MutualFunds implements OnInit {
  private readonly api = inject(WealthService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmModalService);
  private readonly search$ = new Subject<string>();

  readonly rows = signal<MutualFund[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly syncing = signal(false);
  readonly showModal = signal(false);
  readonly showImport = signal(false);
  readonly editId = signal<number | null>(null);
  readonly schemeQuery = signal('');
  readonly suggestions = signal<MutualFundSchemeLookup[]>([]);
  readonly searching = signal(false);
  readonly importErrors = signal<string[]>([]);
  readonly importPreview = signal<UpsertMutualFundRequest[]>([]);
  readonly schemeLabels = SCHEME_LABELS;

  form: UpsertMutualFundRequest = this.empty();

  readonly headerActions: PageHeaderAction[] = [
    { id: 'refresh', label: 'Refresh', icon: 'bi-arrow-clockwise', color: 'outline-secondary' },
    { id: 'sync', label: 'Sync NAV', icon: 'bi-cloud-download', color: 'outline-success' },
    { id: 'import', label: 'Import', icon: 'bi-upload', color: 'outline-secondary' },
    { id: 'export', label: 'Export', icon: 'bi-download', color: 'outline-secondary' },
    { id: 'add', label: 'Add scheme', icon: 'bi-plus-lg', color: 'primary' },
  ];

  ngOnInit(): void {
    this.search$
      .pipe(
        debounceTime(280),
        distinctUntilChanged(),
        switchMap((q) => {
          this.searching.set(true);
          return this.api.searchSchemes(q);
        }),
      )
      .subscribe({
        next: (rows) => {
          this.suggestions.set(rows);
          this.searching.set(false);
        },
        error: (err) => {
          this.searching.set(false);
          this.suggestions.set([]);
          this.toast.error(apiErrorMessage(err, 'Scheme search failed'));
        },
      });
    this.load();
  }

  onHeaderAction(id: string): void {
    if (id === 'refresh') this.load();
    if (id === 'add') this.open();
    if (id === 'sync') this.syncAll();
    if (id === 'export') this.exportCsv();
    if (id === 'import') this.showImport.set(true);
  }

  onSchemeQuery(value: string): void {
    this.schemeQuery.set(value);
    this.form.schemeName = value;
    if (value.trim().length >= 2) this.search$.next(value.trim());
    else this.suggestions.set([]);
  }

  pickScheme(item: MutualFundSchemeLookup): void {
    this.form.schemeName = item.schemeName;
    this.form.schemeCode = item.schemeCode;
    if (item.amc) this.form.amc = item.amc;
    this.schemeQuery.set(item.schemeName);
    this.suggestions.set([]);
  }

  open(row?: MutualFund): void {
    this.editId.set(row?.id ?? null);
    this.suggestions.set([]);
    this.form = row
      ? {
          schemeName: row.schemeName,
          amc: row.amc,
          folioNumber: row.folioNumber ?? '',
          schemeCode: row.schemeCode ?? null,
          schemeType: row.schemeType,
          units: row.units,
          averageNav: row.averageNav,
          currentNav: row.currentNav,
          purchaseDate: row.purchaseDate.substring(0, 10),
          notes: row.notes ?? '',
          isActive: row.isActive,
        }
      : this.empty();
    this.schemeQuery.set(this.form.schemeName);
    this.showModal.set(true);
  }

  save(): void {
    if (!this.form.schemeName || this.form.units <= 0 || this.form.averageNav <= 0) {
      this.toast.warning('Scheme, units and average NAV are required');
      return;
    }
    if (!this.form.amc) this.form.amc = 'Unknown AMC';
    this.saving.set(true);
    const req$ = this.editId()
      ? this.api.updateMutualFund(this.editId()!, this.form)
      : this.api.addMutualFund(this.form);
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

  syncAll(): void {
    this.syncing.set(true);
    this.api.syncNav().subscribe({
      next: (res) => {
        this.syncing.set(false);
        const d = res.data;
        this.toast.success(res.message || `Updated ${d?.updated ?? 0}. Failed ${d?.failed ?? 0}.`);
        if (d?.errors?.length) this.toast.warning(d.errors.slice(0, 3).join(' · '));
        this.load();
      },
      error: (err) => {
        this.syncing.set(false);
        this.toast.error(apiErrorMessage(err, 'NAV sync failed'));
      },
    });
  }

  syncOne(row: MutualFund, event?: Event): void {
    event?.stopPropagation();
    if (!row.schemeCode) {
      this.toast.warning('Pick a scheme from search so we have a scheme code.');
      return;
    }
    this.api.syncOneNav(row.id).subscribe({
      next: (res) => {
        this.toast.success(res.message || 'NAV updated');
        this.load();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'NAV sync failed')),
    });
  }

  async remove(row: MutualFund): Promise<void> {
    const ok = await this.confirm.open({ title: 'Delete scheme', message: `Remove ${row.schemeName}?`, confirmText: 'Delete' });
    if (!ok) return;
    this.api.deleteMutualFund(row.id).subscribe({
      next: () => {
        this.toast.success('Deleted');
        this.load();
      },
      error: (err) => this.toast.error(apiErrorMessage(err, 'Delete failed')),
    });
  }

  exportCsv(): void {
    const rows = this.rows();
    if (!rows.length) {
      this.toast.warning('Nothing to export');
      return;
    }
    const header = ['SchemeCode', 'SchemeName', 'Amc', 'Folio', 'Type', 'Units', 'AverageNav', 'CurrentNav', 'PurchaseDate', 'Notes'];
    const lines = [header.join(',')];
    for (const r of rows) {
      lines.push(
        [
          r.schemeCode ?? '',
          `"${r.schemeName.replaceAll('"', '""')}"`,
          `"${(r.amc ?? '').replaceAll('"', '""')}"`,
          r.folioNumber ?? '',
          r.schemeType,
          r.units,
          r.averageNav,
          r.currentNav,
          (r.purchaseDate ?? '').substring(0, 10),
          `"${(r.notes ?? '').replaceAll('"', '""')}"`,
        ].join(','),
      );
    }
    downloadTextFile(`mutual-funds-${new Date().toISOString().slice(0, 10)}.csv`, lines.join('\n'));
    this.toast.success(`Exported ${rows.length} schemes`);
  }

  downloadSample(): void {
    downloadTextFile(
      'mutual-funds-sample.csv',
      'SchemeCode,SchemeName,Amc,Folio,Type,Units,AverageNav,CurrentNav,PurchaseDate,Notes\n122639,Parag Parikh Flexi Cap Fund - Direct Plan - Growth,PPFAS,123456,1,100,70.5,0,2024-01-15,Sample',
    );
  }

  onImportFile(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => this.parseImport(String(reader.result ?? ''));
    reader.readAsText(file);
  }

  submitImport(): void {
    const rows = this.importPreview();
    if (!rows.length) return;
    this.saving.set(true);
    let done = 0;
    let failed = 0;
    const next = (i: number): void => {
      if (i >= rows.length) {
        this.saving.set(false);
        this.toast.success(`Imported ${done}. Failed ${failed}.`);
        this.showImport.set(false);
        this.load();
        return;
      }
      this.api.addMutualFund(rows[i]).subscribe({
        next: (res) => {
          if (res.success) done++;
          else failed++;
          next(i + 1);
        },
        error: () => {
          failed++;
          next(i + 1);
        },
      });
    };
    next(0);
  }

  load(): void {
    this.loading.set(true);
    this.api.mutualFunds().subscribe({
      next: (rows) => {
        this.rows.set(rows);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(apiErrorMessage(err, 'Could not load mutual funds'));
      },
    });
  }

  private parseImport(text: string): void {
    const lines = csvLines(text);
    const errors: string[] = [];
    const preview: UpsertMutualFundRequest[] = [];
    if (lines.length < 2) {
      this.importErrors.set(['CSV needs a header and at least one row']);
      this.importPreview.set([]);
      return;
    }
    const headers = csvHeaders(lines[0]);
    const codeIdx = headerIndex(headers, 'schemecode', 'scheme code', 'code');
    const nameIdx = headerIndex(headers, 'schemename', 'scheme', 'name');
    const amcIdx = headerIndex(headers, 'amc', 'house');
    const folioIdx = headerIndex(headers, 'folio');
    const typeIdx = headerIndex(headers, 'type');
    const unitsIdx = headerIndex(headers, 'unit');
    const avgIdx = headerIndex(headers, 'average', 'avg');
    const navIdx = headerIndex(headers, 'currentnav', 'nav');
    const dateIdx = headerIndex(headers, 'purchase', 'date');
    const notesIdx = headerIndex(headers, 'note');
    if (nameIdx === -1 || unitsIdx === -1 || avgIdx === -1) {
      this.importErrors.set(['Required columns: SchemeName, Units, AverageNav']);
      return;
    }
    for (let i = 1; i < lines.length; i++) {
      const cols = parseCsvLine(lines[i]);
      const name = cols[nameIdx] ?? '';
      const units = Number(cols[unitsIdx] ?? 0);
      const avg = Number(cols[avgIdx] ?? 0);
      if (!name || units <= 0 || avg <= 0) {
        errors.push(`Row ${i + 1}: scheme, units and average NAV required`);
        continue;
      }
      preview.push({
        schemeName: name,
        amc: amcIdx >= 0 ? cols[amcIdx] || 'Unknown AMC' : 'Unknown AMC',
        folioNumber: folioIdx >= 0 ? cols[folioIdx] : '',
        schemeCode: codeIdx >= 0 && cols[codeIdx] ? Number(cols[codeIdx]) : null,
        schemeType: (typeIdx >= 0 ? (Number(cols[typeIdx]) as 1 | 2 | 3 | 4 | 5) : 1) || 1,
        units,
        averageNav: avg,
        currentNav: navIdx >= 0 ? Number(cols[navIdx] || 0) : 0,
        purchaseDate: dateIdx >= 0 ? cols[dateIdx] : new Date().toISOString().substring(0, 10),
        notes: notesIdx >= 0 ? cols[notesIdx] : '',
        isActive: true,
      });
    }
    this.importErrors.set(errors);
    this.importPreview.set(preview);
  }

  private empty(): UpsertMutualFundRequest {
    return {
      schemeName: '',
      amc: '',
      folioNumber: '',
      schemeCode: null,
      schemeType: 1,
      units: 0,
      averageNav: 0,
      currentNav: 0,
      purchaseDate: new Date().toISOString().substring(0, 10),
      notes: '',
      isActive: true,
    };
  }
}
