export type FpModalSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'full';

export type FpModalVariant = 'default' | 'danger' | 'success' | 'warning';

/** Named widths. Override any instance with [width]="'480px'" or [width]="420". */
export const FP_MODAL_WIDTH: Record<FpModalSize, string> = {
  xs: '360px',
  sm: '420px',
  md: '520px',
  lg: '720px',
  xl: '920px',
  full: '1100px',
};
