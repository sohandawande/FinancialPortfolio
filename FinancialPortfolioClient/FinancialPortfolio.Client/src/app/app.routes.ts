import { Routes } from '@angular/router';

import { authGuard } from '../core/guards/auth/auth.guard';
import { roleGuard } from '../core/guards/auth/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('../auth/login/login').then((c) => c.Login),
  },
  {
    path: 'register',
    loadComponent: () => import('../auth/register/register').then((c) => c.Register),
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('../auth/forgot-password/forgot-password').then((c) => c.ForgotPassword),
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('../auth/reset-password/reset-password').then((c) => c.ResetPassword),
  },
  {
    path: 'access-denied',
    loadComponent: () => import('../auth/access-denied/access-denied').then((c) => c.AccessDenied),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('../layout/admin-layout/admin-layout').then((c) => c.AdminLayout),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () => import('../features/dashboard/dashboard').then((c) => c.Dashboard),
      },
      {
        path: 'system-logs',
        loadComponent: () =>
          import('../features/logs/system-logs/system-logs').then((c) => c.SystemLogs),
      },
      {
        path: 'system-logs/:id',
        loadComponent: () =>
          import('../features/logs/system-log-details/system-log-details').then(
            (c) => c.SystemLogDetails,
          ),
      },
      {
        path: 'users/pending',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('../features/users/pending-users/pending-users').then((c) => c.PendingUsers),
      },
      {
        path: 'users/manage',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('../features/users/manage-users/manage-users').then((c) => c.ManageUsers),
      },
      {
        path: 'users/manage/:id',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('../features/users/user-details/user-details').then((c) => c.UserDetails),
      },
      {
        path: 'users',
        redirectTo: 'users/pending',
        pathMatch: 'full',
      },
      {
        path: 'stocks',
        loadComponent: () =>
          import('../features/stocks/stock-lists/stock-lists').then((c) => c.StockLists),
      },
      {
        path: 'stocks/:id',
        loadComponent: () =>
          import('../features/stocks/stock-details/stock-details').then((c) => c.StockDetails),
      },
      {
        path: 'etfs',
        loadComponent: () => import('../features/etfs/etf-lists/etf-lists').then((c) => c.EtfLists),
      },
      {
        path: 'etfs/:id',
        loadComponent: () =>
          import('../features/etfs/etf-details/etf-details').then((c) => c.EtfDetails),
      },
      {
        path: 'change-password',
        loadComponent: () =>
          import('../auth/change-password/change-password').then((c) => c.ChangePassword),
      },
      {
        path: 'portfolio',
        loadComponent: () => import('../features/portfolio/portfolio').then((c) => c.Portfolio),
      },
      {
        path: 'holdings',
        loadComponent: () =>
          import('../features/holdings/holdings-list/holdings-list').then((c) => c.HoldingsList),
      },
      {
        path: 'holdings/:stockId',
        loadComponent: () =>
          import('../features/holdings/holding-details/holding-details').then(
            (c) => c.HoldingDetails,
          ),
      },

      {
        path: 'wealth',
        loadComponent: () => import('../features/wealth/wealth').then((c) => c.Wealth),
      },
      {
        path: 'mutual-funds',
        loadComponent: () =>
          import('../features/mutual-funds/mutual-funds').then((c) => c.MutualFunds),
      },
      {
        path: 'fixed-deposits',
        loadComponent: () =>
          import('../features/fixed-deposits/fixed-deposits').then((c) => c.FixedDeposits),
      },
      {
        path: 'recurring-deposits',
        loadComponent: () =>
          import('../features/recurring-deposits/recurring-deposits').then(
            (c) => c.RecurringDeposits,
          ),
      },
      {
        path: 'dividends',
        loadComponent: () => import('../features/dividends/dividends').then((c) => c.Dividends),
      },
      {
        path: 'insurance-policies',
        loadComponent: () =>
          import('../features/insurance-policies/insurance-policies').then(
            (m) => m.InsurancePolicies,
          ),
      },
      {
        path: 'transactions',
        redirectTo: 'portfolio',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '**',
    loadComponent: () => import('../auth/not-found/not-found').then((c) => c.NotFound),
  },
];
