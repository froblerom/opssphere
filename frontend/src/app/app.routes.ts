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
    path: 'admin/organization',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.OrganizationView, AppPermissions.OrganizationManage]
    },
    loadComponent: () => import('./features/organization/organization-admin.component').then((m) => m.OrganizationAdminComponent)
  },
  {
    path: 'admin/organization/regions',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.OrganizationView],
      kind: 'regions'
    },
    loadComponent: () => import('./features/organization/organization-entity-manager.component').then((m) => m.OrganizationEntityManagerComponent)
  },
  {
    path: 'admin/organization/countries',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.OrganizationView],
      kind: 'countries'
    },
    loadComponent: () => import('./features/organization/organization-entity-manager.component').then((m) => m.OrganizationEntityManagerComponent)
  },
  {
    path: 'admin/organization/accounts',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.OrganizationView],
      kind: 'accounts'
    },
    loadComponent: () => import('./features/organization/organization-entity-manager.component').then((m) => m.OrganizationEntityManagerComponent)
  },
  {
    path: 'admin/organization/campaigns',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.OrganizationView],
      kind: 'campaigns'
    },
    loadComponent: () => import('./features/organization/organization-entity-manager.component').then((m) => m.OrganizationEntityManagerComponent)
  },
  {
    path: 'admin/organization/assignments',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.AssignmentsManage, AppPermissions.ScopesManage]
    },
    loadComponent: () => import('./features/organization/organization-assignments.component').then((m) => m.OrganizationAssignmentsComponent)
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
    path: 'customers',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.CustomersView]
    },
    loadComponent: () => import('./features/customers/customer-list.component').then((m) => m.CustomerListComponent)
  },
  {
    path: 'customers/create',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.CustomersCreate]
    },
    loadComponent: () => import('./features/customers/customer-form.component').then((m) => m.CustomerFormComponent)
  },
  {
    path: 'customers/:id',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.CustomersView]
    },
    loadComponent: () => import('./features/customers/customer-detail.component').then((m) => m.CustomerDetailComponent)
  },
  {
    path: 'customers/:id/edit',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.CustomersUpdate]
    },
    loadComponent: () => import('./features/customers/customer-form.component').then((m) => m.CustomerFormComponent)
  },
  {
    path: 'tickets',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.TicketsView]
    },
    loadComponent: () => import('./features/tickets/ticket-list.component').then((m) => m.TicketListComponent)
  },
  {
    path: 'tickets/create',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.TicketsCreate]
    },
    loadComponent: () => import('./features/tickets/ticket-create.component').then((m) => m.TicketCreateComponent)
  },
  {
    path: 'tickets/escalations',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.TicketsView]
    },
    loadComponent: () => import('./features/tickets/ticket-escalation-queue.component').then((m) => m.TicketEscalationQueueComponent)
  },
  {
    path: 'tickets/:id',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.TicketsView]
    },
    loadComponent: () => import('./features/tickets/ticket-detail.component').then((m) => m.TicketDetailComponent)
  },
  {
    path: 'audit',
    canActivate: [permissionGuard],
    data: {
      permissions: [AppPermissions.AuditView]
    },
    loadComponent: () => import('./features/audit/audit-list.component').then((m) => m.AuditListComponent)
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
