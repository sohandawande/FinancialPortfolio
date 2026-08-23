import {
  Component,
  input,
  output,
  signal,
  computed,
  effect,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableColumn } from '../../../../core/models/query/table-column.model';
import { FilterRequest } from '../../../../core/models/query/query-request.model';
import { FilterOperator } from '../../../../core/models/query/filter-operator.enum';

interface FilterRow {
  id: number;
  field: string;
  operator: FilterOperator;
  value: string;
}

interface OperatorOption {
  value: FilterOperator;
  label: string;
}

@Component({
  selector: 'app-data-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './data-filters.html',
  styleUrl: './data-filters.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataFilters {
  readonly columns = input<TableColumn[]>([]);
  /** true = panel only (toggle is on data-grid) */
  readonly embedded = input<boolean>(false);
  readonly filterChange = output<FilterRequest[]>();

  readonly expanded = signal(false);
  readonly rows = signal<FilterRow[]>([]);
  readonly closed = output<void>();

  private seq = 0;

  readonly filterableColumns = computed(() =>
    this.columns().filter((c) => c.filterable && c.type !== 'actions')
  );

  readonly activeCount = computed(
    () => this.rows().filter((r) => r.field && r.value.trim()).length
  );

  readonly operators: OperatorOption[] = [
    { value: FilterOperator.Contains, label: 'Contains' },
    { value: FilterOperator.Equals, label: 'Equals' },
    { value: FilterOperator.NotEquals, label: 'Not Equals' },
    { value: FilterOperator.StartsWith, label: 'Starts With' },
    { value: FilterOperator.EndsWith, label: 'Ends With' },
    { value: FilterOperator.GreaterThan, label: 'Greater Than' },
    { value: FilterOperator.GreaterThanOrEqual, label: 'Greater or Equal' },
    { value: FilterOperator.LessThan, label: 'Less Than' },
    { value: FilterOperator.LessThanOrEqual, label: 'Less or Equal' },
  ];

  constructor() {
    // When used embedded, always show panel and ensure one row
    effect(() => {
      if (this.embedded()) {
        this.expanded.set(true);
        if (this.rows().length === 0 && this.filterableColumns().length > 0) {
          this.addRow();
        }
      }
    });
  }

  toggle(): void {
    this.expanded.update((v) => !v);
    if (this.expanded() && this.rows().length === 0) {
      this.addRow();
    }
  }

  addRow(): void {
    const cols = this.filterableColumns();
    this.rows.update((list) => [
      ...list,
      {
        id: ++this.seq,
        field: cols[0]?.key ?? '',
        operator: cols[0]?.filterOperator ?? FilterOperator.Contains,
        value: '',
      },
    ]);
  }

  removeRow(id: number): void {
  const remaining = this.rows().filter((r) => r.id !== id);

  // Last row: clear filters and collapse (parent closes panel)
  if (remaining.length === 0) {
    this.rows.set([]);
    this.filterChange.emit([]);
    this.expanded.set(false);
    this.closed.emit(); // tell data-grid to close panel
    return;
  }

  this.rows.set(remaining);
}

  updateRow(id: number, patch: Partial<FilterRow>): void {
    this.rows.update((list) =>
      list.map((r) => (r.id === id ? { ...r, ...patch } : r))
    );
  }

  onFieldChange(id: number, field: string): void {
    const col = this.filterableColumns().find((c) => c.key === field);
    this.updateRow(id, {
      field,
      operator: col?.filterOperator ?? FilterOperator.Contains,
    });
  }

  apply(): void {
    const filters: FilterRequest[] = this.rows()
      .filter((r) => r.field && r.value.trim())
      .map((r) => ({
        field: r.field,
        operator: r.operator,
        value: r.value.trim(),
      }));

    this.filterChange.emit(filters);
  }

  clear(): void {
    this.rows.set([]);
    this.filterChange.emit([]);
    this.addRow();
  }
}