import { FilterOperator } from './filter-operator.enum';
import { SortDirection } from './sort-direction.enum';

export interface FilterRequest {
  field: string;
  operator: FilterOperator;
  value: string;
}

export interface SortRequest {
  field: string;
  direction: SortDirection;
}

export interface QueryRequest {
  globalSearch?: string | null;
  filters?: FilterRequest[] | null;
  sorts?: SortRequest[] | null;
  pageNumber: number;
  pageSize: number;
}