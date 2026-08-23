import { SidebarMenu } from '../../models/sidebar-menu/sidebar-menu.model';

export const SIDEBAR_MENU: SidebarMenu[] = [
  {
    id: 1,
    title: 'Dashboard',
    icon: 'bi-speedometer2',
    route: '/dashboard',
  },
  {
    id: 2,
    title: 'Stocks',
    icon: 'bi-graph-up-arrow',
    route: '/stocks',
  },
  {
    id: 3,
    title: 'Portfolio',
    icon: 'bi-briefcase',
    route: '/portfolio',
  },
  {
    id: 4,
    title: 'Holdings',
    icon: 'bi-layers',
    route: '/holdings',
  },
  {
    id: 5,
    title: 'Dividends',
    icon: 'bi-cash-coin',
    route: '/dividends',
  },
  {
    id: 6,
    title: 'Transactions',
    icon: 'bi-arrow-left-right',
    route: '/transactions',
  },
  {
    id: 7,
    title: 'Users',
    icon: 'bi-people',
    route: '/users/manage',
  },
  {
    id: 8,
    title: 'System Logs',
    icon: 'bi-journal-text',
    route: '/system-logs',
  },
];
