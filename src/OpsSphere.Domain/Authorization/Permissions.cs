namespace OpsSphere.Domain.Authorization;

public static class Permissions
{
    // Identity and access
    public const string UsersView = "users.view";
    public const string UsersManage = "users.manage";
    public const string RolesView = "roles.view";
    public const string RolesManage = "roles.manage";
    public const string PermissionsView = "permissions.view";
    public const string PermissionsManage = "permissions.manage";
    public const string ScopesView = "scopes.view";
    public const string ScopesManage = "scopes.manage";

    // Organization
    public const string OrganizationView = "organization.view";
    public const string OrganizationManage = "organization.manage";
    public const string RegionsManage = "regions.manage";
    public const string CountriesManage = "countries.manage";
    public const string AccountsManage = "accounts.manage";
    public const string CampaignsManage = "campaigns.manage";
    public const string AssignmentsManage = "assignments.manage";

    // Customers
    public const string CustomersView = "customers.view";
    public const string CustomersCreate = "customers.create";
    public const string CustomersUpdate = "customers.update";
    public const string CustomersHistoryView = "customers.history.view";

    // Tickets
    public const string TicketsView = "tickets.view";
    public const string TicketsCreate = "tickets.create";
    public const string TicketsUpdate = "tickets.update";
    public const string TicketsAssign = "tickets.assign";
    public const string TicketsUpdateStatus = "tickets.update_status";
    public const string TicketsUpdatePriority = "tickets.update_priority";
    public const string TicketsComment = "tickets.comment";
    public const string TicketsEscalate = "tickets.escalate";
    public const string TicketsResolve = "tickets.resolve";
    public const string TicketsClose = "tickets.close";
    public const string TicketsHistoryView = "tickets.history.view";
    public const string TicketsReopen = "tickets.reopen";

    // SLA
    public const string SlaView = "sla.view";
    public const string SlaPoliciesView = "sla.policies.view";
    public const string SlaPoliciesManage = "sla.policies.manage";
    public const string SlaEvaluate = "sla.evaluate";

    // Dashboard and reports
    public const string DashboardView = "dashboard.view";
    public const string ReportsView = "reports.view";
    public const string ReportsExport = "reports.export";

    // Audit
    public const string AuditView = "audit.view";
    public const string AuditAdminView = "audit.admin_view";
    public const string AuditExport = "audit.export";

    public static readonly IReadOnlyList<string> All =
    [
        UsersView, UsersManage, RolesView, RolesManage, PermissionsView, PermissionsManage,
        ScopesView, ScopesManage, OrganizationView, OrganizationManage, RegionsManage,
        CountriesManage, AccountsManage, CampaignsManage, AssignmentsManage,
        CustomersView, CustomersCreate, CustomersUpdate, CustomersHistoryView,
        TicketsView, TicketsCreate, TicketsUpdate, TicketsAssign, TicketsUpdateStatus,
        TicketsUpdatePriority, TicketsComment, TicketsEscalate, TicketsResolve, TicketsClose,
        TicketsHistoryView, TicketsReopen, SlaView, SlaPoliciesView, SlaPoliciesManage,
        SlaEvaluate, DashboardView, ReportsView, ReportsExport, AuditView, AuditAdminView,
        AuditExport
    ];
}
