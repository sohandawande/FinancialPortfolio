/**
 * Shared Bootstrap button color tokens used by:
 * - TableAction (data-grid row actions)
 * - PageHeaderAction (page header buttons)
 *
 * Use solid: 'primary' → btn-primary
 * Use outline: 'outline-secondary' → btn-outline-secondary
 */
export type BtnColorType =
  | 'primary'
  | 'secondary'
  | 'success'
  | 'danger'
  | 'warning'
  | 'info'
  | 'light'
  | 'dark'
  | 'outline-primary'
  | 'outline-secondary'
  | 'outline-success'
  | 'outline-danger'
  | 'outline-warning'
  | 'outline-info'
  | 'outline-light'
  | 'outline-dark'
  | 'link';

/** Default for subtle row actions in data tables */
export const DEFAULT_TABLE_ACTION_COLOR: BtnColorType = 'outline-primary';

/** Default for page header secondary actions */
export const DEFAULT_HEADER_ACTION_COLOR: BtnColorType = 'outline-secondary';