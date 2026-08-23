export const ROLE_CONSTANTS = {
  Pending: 'Pending',
  Admin: 'Admin',
  User: 'User',
  PortfolioManager: 'PortfolioManager',
  Trader: 'Trader',
  Viewer: 'Viewer',
} as const;

/** Roles admin can assign on approve (exclude Pending) */
export const ASSIGNABLE_ROLES = [
  ROLE_CONSTANTS.User,
  ROLE_CONSTANTS.PortfolioManager,
  ROLE_CONSTANTS.Trader,
  ROLE_CONSTANTS.Viewer,
  ROLE_CONSTANTS.Admin,
] as const;