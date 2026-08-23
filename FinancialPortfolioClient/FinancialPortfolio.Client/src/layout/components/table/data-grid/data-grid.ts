import { Component, input, output, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DataSearchBox } from '../data-search-box/data-search-box';
import { DataColumnToggler } from '../data-column-toggler/data-column-toggler';
import { DataFilters } from '../data-filters/data-filters';
import { DataTable } from '../data-table/data-table';
import { DataPagination } from '../data-pagination/data-pagination';

import { TableColumn } from '../../../../core/models/query/table-column.model';
import {
  QueryRequest,
  FilterRequest,
  SortRequest,
} from '../../../../core/models/query/query-request.model';
import { DataGridMode } from '../../../../core/types/query/data-grid-mode.type';
import { applyClientQuery } from '../../../../core/utilities/query/apply-client-query';

@Component({
  selector: 'app-data-grid',
  standalone: true,
  imports: [CommonModule, DataSearchBox, DataColumnToggler, DataFilters, DataTable, DataPagination],
  templateUrl: './data-grid.html',
  styleUrl: './data-grid.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataGrid<T extends object = any> {
  readonly title = input<string>('');
  readonly columns = input<TableColumn<T>[]>([]);
  readonly data = input<T[]>([]);
  readonly loading = input<boolean>(false);
  readonly totalRecords = input<number>(0);
  readonly emptyMessage = input<string>('No records found');
  readonly query = input<QueryRequest>({ pageNumber: 1, pageSize: 10 });

  readonly mode = input<DataGridMode>('server');
  readonly searchKeys = input<string[]>([]);
  readonly embedded = input<boolean>(false);

  readonly showSearch = input<boolean>(true);
  readonly showFilters = input<boolean>(true);
  readonly showColumnToggler = input<boolean>(true);
  readonly showPagination = input<boolean>(true);
  readonly showSerial = input<boolean>(true);

  readonly queryChange = output<QueryRequest>();
  readonly rowClick = output<T>();
  readonly columnsChange = output<TableColumn<T>[]>();

  readonly filtersOpen = signal(false);
  readonly activeFilterCount = signal(0);

  readonly resolved = computed(() => {
    if (this.mode() !== 'client') {
      return { page: this.data(), total: this.totalRecords() };
    }
    return applyClientQuery(this.data(), this.query(), this.searchKeys());
  });

  toggleFilters(): void {
    this.filtersOpen.update((v) => !v);
  }

  onSearch(term: string): void {
    this.queryChange.emit({
      ...this.query(),
      globalSearch: term || null,
      pageNumber: 1,
    });
  }

  onFilter(filters: FilterRequest[]): void {
    this.activeFilterCount.set(filters?.length ?? 0);
    this.queryChange.emit({
      ...this.query(),
      filters: filters?.length ? filters : null,
      pageNumber: 1,
    });
  }

  onFiltersClosed(): void {
    this.filtersOpen.set(false);
    this.activeFilterCount.set(0);
  }

  onSort(sorts: SortRequest[]): void {
    this.queryChange.emit({
      ...this.query(),
      sorts: sorts?.length ? sorts : null,
      pageNumber: 1,
    });
  }

  onPage(page: number): void {
    this.queryChange.emit({
      ...this.query(),
      pageNumber: page,
    });
  }

  onPageSize(size: number): void {
    this.queryChange.emit({
      ...this.query(),
      pageSize: size,
      pageNumber: 1,
    });
  }

  onColumnsChange(cols: TableColumn<T>[]): void {
    this.columnsChange.emit(cols);
  }
}
