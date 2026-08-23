import { FilterOperator } from './filter-operator.enum';
import { BtnColorType } from '../../types/common/btn-color-type.type';
import { ColumnType } from '../../types/query/column-type.type';

export interface TableAction<T = any> {
  icon: string;
  label?: string;
  color?: BtnColorType;
  visible?: (row: T) => boolean;
  disabled?: (row: T) => boolean;
  click: (row: T) => void;
}

export interface TableColumn<T = any> {
  key: string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  filterOperator?: FilterOperator;
  type?: ColumnType;
  width?: string;
  align?: 'start' | 'center' | 'end';
  hidden?: boolean;
  canToggle?: boolean;
  formatter?: (row: T) => string | number;
  actions?: TableAction<T>[];
  badgeClass?: (row: T) => string;
  percentKey?: string;
  symbolKey?: string;
  nameKey?: string;
  logoKey?: string;
}
