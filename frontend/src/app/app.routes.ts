import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./home-placeholder.component').then((m) => m.HomePlaceholderComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./home-placeholder.component').then((m) => m.HomePlaceholderComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
