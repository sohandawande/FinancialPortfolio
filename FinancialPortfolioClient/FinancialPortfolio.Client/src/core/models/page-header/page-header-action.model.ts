import { BtnColorType } from "../../types/common/btn-color-type.type";

export interface PageHeaderAction {
  id: string;
  label: string;
  icon?: string;           // e.g. 'bi-arrow-clockwise'
  color?: BtnColorType;
  disabled?: boolean;
  loading?: boolean;
  visible?: boolean;       // default true
  title?: string;          // native tooltip
}