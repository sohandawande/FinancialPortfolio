import { SidebarMenu } from '../../models/sidebar-menu/sidebar-menu.model';

export const SIDEBAR_MENU: SidebarMenu[] = [
  { id: 1, title: 'Dashboard', icon: 'bi-speedometer2', route: '/dashboard' },
  { id: 2, title: 'Wealth', icon: 'bi-pie-chart-fill', route: '/wealth' },
  { id: 3, title: 'Stocks', icon: 'bi-graph-up-arrow', route: '/stocks' },
  { id: 4, title: 'Portfolio', icon: 'bi-briefcase', route: '/portfolio' },
  { id: 5, title: 'Holdings', icon: 'bi-layers', route: '/holdings' },
  { id: 6, title: 'Mutual funds', icon: 'bi-pie-chart', route: '/mutual-funds' },
  { id: 7, title: 'Fixed deposits', icon: 'bi-bank', route: '/fixed-deposits' },
  { id: 8, title: 'Recurring deposits', icon: 'bi-calendar-check', route: '/recurring-deposits' },
  { id: 9, title: 'Dividends', icon: 'bi-cash-coin', route: '/dividends' },
  { id: 10, title: 'Users', icon: 'bi-people', route: '/users/manage' },
  { id: 11, title: 'System Logs', icon: 'bi-journal-text', route: '/system-logs' },
];
