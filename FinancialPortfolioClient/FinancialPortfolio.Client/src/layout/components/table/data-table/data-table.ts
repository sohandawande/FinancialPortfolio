import {
  Component,
  input,
  output,
  computed,
  inject,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { TableColumn, TableAction } from '../../../../core/models/query/table-column.model';
import { SortRequest } from '../../../../core/models/query/query-request.model';
import { SortDirection } from '../../../../core/models/query/sort-direction.enum';
import { StockLogo } from '../../stock-logo/stock-logo';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, StockLogo],
  providers: [DecimalPipe],
  templateUrl: './data-table.html',
  styleUrl: './data-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTable<T = any> {
  private readonly decimal = inject(DecimalPipe);

  readonly columns = input<TableColumn<T>[]>([]);
  readonly data = input<T[]>([]);
  readonly loading = input(false);
  readonly emptyMessage = input('No records found');
  readonly sorts = input<SortRequest[]>([]);

  readonly pageNumber = input<number>(1);
  readonly pageSize = input<number>(10);
  readonly showSerial = input<boolean>(true);

  readonly sortChange = output<SortRequest[]>();
  readonly rowClick = output<T>();

  readonly visibleColumns = computed(() => this.columns().filter((c) => !c.hidden));

  serialNo(index: number): number {
    return (this.pageNumber() - 1) * this.pageSize() + index + 1;
  }

  onSort(col: TableColumn): void {
    if (!col.sortable || col.type === 'actions') return;

    const current = this.sorts();
    const existing = current.find((s) => s.field === col.key);

    let next: SortRequest[];
    if (!existing) {
      next = [{ field: col.key, direction: SortDirection.Asc }];
    } else if (existing.direction === SortDirection.Asc) {
      next = [{ field: col.key, direction: SortDirection.Desc }];
    } else {
      next = [];
    }

    this.sortChange.emit(next);
  }

  getSortIcon(col: TableColumn): string {
    const sort = this.sorts().find((s) => s.field === col.key);
    if (!sort) return 'bi-arrow-down-up text-muted';
    return sort.direction === SortDirection.Asc ? 'bi-sort-up' : 'bi-sort-down';
  }

  getValue(row: T, col: TableColumn): unknown {
    if (col.formatter) return col.formatter(row);
    return (row as Record<string, unknown>)?.[col.key] ?? '';
  }

  formatMoney(value: unknown): string {
    const n = Number(value);
    if (!Number.isFinite(n)) return '—';
    return `₹${this.decimal.transform(n, '1.2-2') ?? '0.00'}`;
  }

  formatPnL(value: unknown): string {
    const n = Number(value);
    if (!Number.isFinite(n)) return '—';
    const sign = n > 0 ? '+' : n < 0 ? '−' : '';
    return `${sign}₹${this.decimal.transform(Math.abs(n), '1.2-2')}`;
  }

  formatPercent(value: unknown): string {
    const n = Number(value);
    if (!Number.isFinite(n)) return '—';
    const sign = n > 0 ? '+' : n < 0 ? '−' : '';
    return `${sign}${this.decimal.transform(Math.abs(n), '1.2-2')}%`;
  }

  formatDate(value: unknown): string {
    if (!value) return '—';
    const raw = String(value);
    const d = new Date(raw);
    if (Number.isNaN(d.getTime())) return raw;
    return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  numeric(row: T, key: string): number {
    return Number((row as Record<string, unknown>)?.[key] ?? 0);
  }

  stockSymbol(row: T, col: TableColumn): string {
    return String((row as Record<string, unknown>)[col.symbolKey || 'symbol'] ?? '');
  }

  stockName(row: T, col: TableColumn): string {
    return String((row as Record<string, unknown>)[col.nameKey || 'companyName'] ?? '');
  }

  stockLogo(row: T, col: TableColumn): string | null {
    return ((row as Record<string, unknown>)[col.logoKey || 'logoUrl'] as string | null) ?? null;
  }

  alignClass(col: TableColumn): string {
    if (col.align === 'end') return 'text-end dt-num';
    if (col.align === 'center') return 'text-center';
    const key = (col.key || '').toLowerCase();
    if (key.includes('status') || key.includes('day')) return 'text-center';
    if (col.type === 'money' || col.type === 'pnl' || col.type === 'number') return 'text-end dt-num';
    if (col.type === 'badge' || col.type === 'actions' || col.type === 'date') return 'text-center';
    return '';
  }

  isEnd(col: TableColumn): boolean {
    return this.alignClass(col).includes('text-end');
  }

  isCenter(col: TableColumn): boolean {
    return this.alignClass(col).includes('text-center');
  }

  isHtml(value: unknown): boolean {
    return typeof value === 'string' && value.includes('<');
  }

  isActionVisible(action: TableAction<T>, row: T): boolean {
    return action.visible ? action.visible(row) : true;
  }

  isActionDisabled(action: TableAction<T>, row: T): boolean {
    return action.disabled ? action.disabled(row) : false;
  }

  onAction(event: Event, action: TableAction<T>, row: T): void {
    event.stopPropagation();
    if (!this.isActionDisabled(action, row)) {
      action.click(row);
    }
  }
}
