import { Routes } from '@angular/router';

import { authGuard, permissionGuard } from './core/guards/auth.guard';
import { AppPermissions, AppRoles } from './core/auth/auth-permissions';

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
    path: 'admin',
    canActivate: [permissionGuard],
    data: {
      roles: [AppRoles.Admin],
      permissions: [AppPermissions.UsersView]
    },
    loadComponent: () => import('./features/users/admin-users.component').then((m) => m.AdminUsersComponent)
  },
  {
    path: 'admin/users',
    canActivate: [permissionGuard],
    data: {
      roles: [AppRoles.Admin],
      permissions: [AppPermissions.UsersView]
    },
    loadComponent: () => import('./features/users/user-list.component').then((m) => m.UserListComponent)
  },
  {
    path: 'admin/users/create',
    canActivate: [permissionGuard],
    data: {
      roles: [AppRoles.Admin],
      permissions: [AppPermissions.UsersManage]
    },
    loadComponent: () => import('./features/users/user-form.component').then((m) => m.UserFormComponent)
  },
  {
    path: 'admin/users/:id',
    canActivate: [permissionGuard],
    data: {
      roles: [AppRoles.Admin],
      permissions: [AppPermissions.UsersView]
    },
    loadComponent: () => import('./features/users/user-detail.component').then((m) => m.UserDetailComponent)
  },
  {
    path: 'admin/users/:id/edit',
    canActivate: [permissionGuard],
    data: {
      roles: [AppRoles.Admin],
      permissions: [AppPermissions.UsersManage]
    },
    loadComponent: () => import('./features/users/user-form.component').then((m) => m.UserFormComponent)
  },
  {
    path: 'admin/users/:id/roles',
    canActivate: [permissionGuard],
    data: {
      roles: [AppRoles.Admin],
      permissions: [AppPermissions.RolesManage]
    },
    loadComponent: () => import('./features/users/user-roles.component').then((m) => m.UserRolesComponent)
  },
  {
    path: 'admin/roles',
    canActivate: [permissionGuard],
    data: {
      roles: [AppRoles.Admin],
      permissions: [AppPermissions.RolesView]
    },
    loadComponent: () => import('./features/users/role-permission-list.component').then((m) => m.RolePermissionListComponent)
  },
  {
    path: 'users',
    redirectTo: 'admin/users'
  },
  {
    path: '**',
    redirectTo: ''
  }
];
