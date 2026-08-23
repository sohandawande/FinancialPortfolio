export interface TableAction<T = any> {
  icon: string;
  label?: string;
  color?: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info';
  visible?: (row: T) => boolean;
  disabled?: (row: T) => boolean;
  click: (row: T) => void;
}