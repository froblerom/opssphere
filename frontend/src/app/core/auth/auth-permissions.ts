// Frontend permission constants — must match backend OpsSphere.Domain.Authorization.Permissions.
// SECURITY NOTE: These are for UX visibility only. Backend authorization is the source of truth.
// Do not use these to make security decisions — only to improve UX by hiding unavailable actions.

export const AppPermissions = {
  UsersView: 'users.view',
  UsersManage: 'users.manage',
  RolesView: 'roles.view',
  RolesManage: 'roles.manage',
  PermissionsView: 'permissions.view',
  PermissionsManage: 'permissions.manage',
  ScopesView: 'scopes.view',
  ScopesManage: 'scopes.manage',

  OrganizationView: 'organization.view',
  OrganizationManage: 'organization.manage',
  RegionsManage: 'regions.manage',
  CountriesManage: 'countries.manage',
  AccountsManage: 'accounts.manage',
  CampaignsManage: 'campaigns.manage',
  AssignmentsManage: 'assignments.manage',

  CustomersView: 'customers.view',
  CustomersCreate: 'customers.create',
  CustomersUpdate: 'customers.update',
  CustomersHistoryView: 'customers.history.view',

  TicketsView: 'tickets.view',
  TicketsCreate: 'tickets.create',
  TicketsUpdate: 'tickets.update',
  TicketsAssign: 'tickets.assign',
  TicketsUpdateStatus: 'tickets.update_status',
  TicketsUpdatePriority: 'tickets.update_priority',
  TicketsComment: 'tickets.comment',
  TicketsEscalate: 'tickets.escalate',
  TicketsResolve: 'tickets.resolve',
  TicketsClose: 'tickets.close',
  TicketsHistoryView: 'tickets.history.view',
  TicketsReopen: 'tickets.reopen',

  SlaView: 'sla.view',
  SlaPoliciesView: 'sla.policies.view',
  SlaPoliciesManage: 'sla.policies.manage',
  SlaEvaluate: 'sla.evaluate',

  DashboardView: 'dashboard.view',
  ReportsView: 'reports.view',
  ReportsExport: 'reports.export',

  AuditView: 'audit.view',
  AuditAdminView: 'audit.admin_view',
  AuditExport: 'audit.export',
} as const;

export type AppPermission = (typeof AppPermissions)[keyof typeof AppPermissions];

// Runtime role name constants — must match backend OpsSphere.Domain.Authorization.Roles.
// SECURITY NOTE: For UX navigation only. Backend enforces role authorization.
export const AppRoles = {
  Admin: 'Admin',
  OperationsManager: 'OperationsManager',
  Supervisor: 'Supervisor',
  Agent: 'Agent',
  Viewer: 'Viewer',
} as const;

export type AppRole = (typeof AppRoles)[keyof typeof AppRoles];
