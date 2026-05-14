import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./home-placeholder.component').then((m) => m.HomePlaceholderComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
