import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    loadComponent: () => import('../../auth/login/login').then((c) => c.Login),
  },

  {
    path: 'forgot-password',
    loadComponent: () =>
      import('../../auth/forgot-password/forgot-password').then((c) => c.ForgotPassword),
  },

  {
    path: 'reset-password',
    loadComponent: () =>
      import('../../auth/reset-password/reset-password').then((c) => c.ResetPassword),
  },

  {
    path: 'change-password',
    loadComponent: () =>
      import('../../auth/change-password/change-password').then((c) => c.ChangePassword),
  },

  {
    path: 'access-denied',
    loadComponent: () =>
      import('../../auth/access-denied/access-denied').then((c) => c.AccessDenied),
  },
];
