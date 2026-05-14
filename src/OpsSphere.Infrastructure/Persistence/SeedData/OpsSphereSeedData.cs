namespace OpsSphere.Infrastructure.Persistence.SeedData;

public static class OpsSphereSeedData
{
    public const string LocalDemoPassword = "OpsSphere123!";

    public static readonly IReadOnlyList<RoleSeed> Roles =
    [
        new(SeedIds.Roles.Admin, "Admin", "Manages platform configuration, users, roles, permissions, scopes, organization, audit, SLA policy, and reports."),
        new(SeedIds.Roles.OperationsManager, "OperationsManager", "Oversees regional operational performance within assigned scope."),
        new(SeedIds.Roles.Supervisor, "Supervisor", "Manages account or campaign ticket operations, assignments, escalations, SLA risk, and agents within scope."),
        new(SeedIds.Roles.Agent, "Agent", "Handles tickets within assigned operational scope."),
        new(SeedIds.Roles.Viewer, "Viewer", "Views operational data, dashboards, reports, and audit history in read-only mode within scope.")
    ];

    public static readonly IReadOnlyList<PermissionSeed> Permissions =
    [
        new(SeedIds.Permissions.UsersView, "users.view", "View Users", "View users."),
        new(SeedIds.Permissions.UsersManage, "users.manage", "Manage Users", "Create, update, deactivate, and manage users."),
        new(SeedIds.Permissions.RolesView, "roles.view", "View Roles", "View roles."),
        new(SeedIds.Permissions.RolesManage, "roles.manage", "Manage Roles", "Manage roles."),
        new(SeedIds.Permissions.PermissionsView, "permissions.view", "View Permissions", "View permissions."),
        new(SeedIds.Permissions.PermissionsManage, "permissions.manage", "Manage Permissions", "Manage role-permission assignments."),
        new(SeedIds.Permissions.ScopesView, "scopes.view", "View Scopes", "View user scopes."),
        new(SeedIds.Permissions.ScopesManage, "scopes.manage", "Manage Scopes", "Assign or update user operational scopes."),
        new(SeedIds.Permissions.OrganizationView, "organization.view", "View Organization", "View regions, countries, accounts, and campaigns."),
        new(SeedIds.Permissions.OrganizationManage, "organization.manage", "Manage Organization", "Create, update, deactivate, or assign organization records."),
        new(SeedIds.Permissions.RegionsManage, "regions.manage", "Manage Regions", "Manage regions."),
        new(SeedIds.Permissions.CountriesManage, "countries.manage", "Manage Countries", "Manage countries."),
        new(SeedIds.Permissions.AccountsManage, "accounts.manage", "Manage Accounts", "Manage accounts."),
        new(SeedIds.Permissions.CampaignsManage, "campaigns.manage", "Manage Campaigns", "Manage campaigns."),
        new(SeedIds.Permissions.AssignmentsManage, "assignments.manage", "Manage Assignments", "Manage manager, supervisor, agent, and viewer assignments."),
        new(SeedIds.Permissions.CustomersView, "customers.view", "View Customers", "View customer records within scope."),
        new(SeedIds.Permissions.CustomersCreate, "customers.create", "Create Customers", "Create customer records within scope."),
        new(SeedIds.Permissions.CustomersUpdate, "customers.update", "Update Customers", "Update customer records within scope."),
        new(SeedIds.Permissions.CustomersHistoryView, "customers.history.view", "View Customer History", "View customer ticket history within scope."),
        new(SeedIds.Permissions.TicketsView, "tickets.view", "View Tickets", "View tickets within scope."),
        new(SeedIds.Permissions.TicketsCreate, "tickets.create", "Create Tickets", "Create tickets within scope."),
        new(SeedIds.Permissions.TicketsUpdate, "tickets.update", "Update Tickets", "Update editable ticket fields within scope."),
        new(SeedIds.Permissions.TicketsAssign, "tickets.assign", "Assign Tickets", "Assign or reassign tickets within scope."),
        new(SeedIds.Permissions.TicketsUpdateStatus, "tickets.update_status", "Update Ticket Status", "Update ticket status within scope."),
        new(SeedIds.Permissions.TicketsUpdatePriority, "tickets.update_priority", "Update Ticket Priority", "Update ticket priority within scope."),
        new(SeedIds.Permissions.TicketsComment, "tickets.comment", "Comment On Tickets", "Add internal comments within scope."),
        new(SeedIds.Permissions.TicketsEscalate, "tickets.escalate", "Escalate Tickets", "Escalate tickets within scope."),
        new(SeedIds.Permissions.TicketsResolve, "tickets.resolve", "Resolve Tickets", "Resolve tickets within scope."),
        new(SeedIds.Permissions.TicketsClose, "tickets.close", "Close Tickets", "Close resolved tickets within scope."),
        new(SeedIds.Permissions.TicketsHistoryView, "tickets.history.view", "View Ticket History", "View ticket history within scope."),
        new(SeedIds.Permissions.TicketsReopen, "tickets.reopen", "Reopen Tickets", "Reopen closed tickets if supported."),
        new(SeedIds.Permissions.SlaView, "sla.view", "View SLA", "View SLA state and SLA dashboards within scope."),
        new(SeedIds.Permissions.SlaPoliciesView, "sla.policies.view", "View SLA Policies", "View SLA policies."),
        new(SeedIds.Permissions.SlaPoliciesManage, "sla.policies.manage", "Manage SLA Policies", "Create or update SLA policies."),
        new(SeedIds.Permissions.SlaEvaluate, "sla.evaluate", "Evaluate SLA", "Trigger or run SLA evaluation."),
        new(SeedIds.Permissions.DashboardView, "dashboard.view", "View Dashboard", "View operational dashboards within scope."),
        new(SeedIds.Permissions.ReportsView, "reports.view", "View Reports", "View reports within scope."),
        new(SeedIds.Permissions.ReportsExport, "reports.export", "Export Reports", "Export reports within scope."),
        new(SeedIds.Permissions.AuditView, "audit.view", "View Audit", "View audit history within scope."),
        new(SeedIds.Permissions.AuditAdminView, "audit.admin_view", "View Admin Audit", "View administrative audit history."),
        new(SeedIds.Permissions.AuditExport, "audit.export", "Export Audit", "Export audit records if enabled.")
    ];

    public static readonly IReadOnlyDictionary<string, string[]> RolePermissions = new Dictionary<string, string[]>
    {
        ["Admin"] =
        [
            "users.view", "users.manage", "roles.view", "roles.manage", "permissions.view", "permissions.manage",
            "scopes.view", "scopes.manage", "organization.view", "organization.manage", "regions.manage",
            "countries.manage", "accounts.manage", "campaigns.manage", "assignments.manage", "customers.view",
            "customers.create", "customers.update", "customers.history.view", "tickets.view", "tickets.history.view",
            "tickets.reopen", "sla.view", "sla.policies.view", "sla.policies.manage", "sla.evaluate", "dashboard.view",
            "reports.view", "reports.export", "audit.view", "audit.admin_view", "audit.export"
        ],
        ["OperationsManager"] =
        [
            "organization.view", "customers.view", "customers.history.view", "tickets.view", "tickets.history.view",
            "tickets.comment", "sla.view", "sla.policies.view", "dashboard.view", "reports.view", "reports.export",
            "audit.view", "audit.export"
        ],
        ["Supervisor"] =
        [
            "organization.view", "customers.view", "customers.create", "customers.update", "customers.history.view",
            "tickets.view", "tickets.create", "tickets.update", "tickets.assign", "tickets.update_status",
            "tickets.update_priority", "tickets.comment", "tickets.escalate", "tickets.resolve", "tickets.close",
            "tickets.history.view", "tickets.reopen", "sla.view", "sla.policies.view", "dashboard.view",
            "reports.view", "reports.export", "audit.view"
        ],
        ["Agent"] =
        [
            "organization.view", "customers.view", "customers.create", "customers.update", "customers.history.view",
            "tickets.view", "tickets.create", "tickets.update", "tickets.update_status", "tickets.update_priority",
            "tickets.comment", "tickets.escalate", "tickets.resolve", "tickets.close", "tickets.history.view",
            "sla.view", "dashboard.view"
        ],
        ["Viewer"] =
        [
            "organization.view", "customers.view", "customers.history.view", "tickets.view", "tickets.history.view",
            "sla.view", "dashboard.view", "reports.view", "reports.export", "audit.view"
        ]
    };

    public static readonly IReadOnlyList<UserSeed> Users =
    [
        new(SeedIds.Users.Admin, "admin@opssphere.local", "Amara", "Vale", "Amara Vale", "Admin"),
        new(SeedIds.Users.ManagerLatam, "manager.latam@opssphere.local", "Mateo", "Rios", "Mateo Rios", "OperationsManager"),
        new(SeedIds.Users.SupervisorNovabank, "supervisor.novabank@opssphere.local", "Lina", "Calderon", "Lina Calderon", "Supervisor"),
        new(SeedIds.Users.AgentNovabank, "agent.novabank@opssphere.local", "Diego", "Santos", "Diego Santos", "Agent"),
        new(SeedIds.Users.ViewerLatam, "viewer.latam@opssphere.local", "Nora", "Ellis", "Nora Ellis", "Viewer")
    ];

    public static readonly IReadOnlyList<RegionSeed> Regions =
    [
        new(SeedIds.Regions.Latam, "LATAM", "Latin America"),
        new(SeedIds.Regions.NorthAmerica, "NA", "North America")
    ];

    public static readonly IReadOnlyList<CountrySeed> Countries =
    [
        new(SeedIds.Countries.Mexico, "MX", "Mexico", "LATAM"),
        new(SeedIds.Countries.Colombia, "CO", "Colombia", "LATAM"),
        new(SeedIds.Countries.CostaRica, "CR", "Costa Rica", "LATAM"),
        new(SeedIds.Countries.UnitedStates, "US", "United States", "NA")
    ];

    public static readonly IReadOnlyList<AccountSeed> Accounts =
    [
        new(SeedIds.Accounts.NovaBank, "NOVABANK", "NovaBank", "MX", "Fictional banking support account for local development."),
        new(SeedIds.Accounts.Streamly, "STREAMLY", "Streamly", "US", "Fictional streaming platform support account for local development."),
        new(SeedIds.Accounts.Shopora, "SHOPORA", "Shopora", "CO", "Fictional commerce support account for local development."),
        new(SeedIds.Accounts.AeroLink, "AEROLINK", "AeroLink", "CR", "Fictional travel support account for local development.")
    ];

    public static readonly IReadOnlyList<CampaignSeed> Campaigns =
    [
        new(SeedIds.Campaigns.NovaBankCreditCard, "NOVABANK-CC", "Credit Card Support", "NOVABANK", "MX", "Fictional credit card support campaign."),
        new(SeedIds.Campaigns.NovaBankFraud, "NOVABANK-FRAUD", "Fraud Review Support", "NOVABANK", "MX", "Fictional fraud review support campaign."),
        new(SeedIds.Campaigns.StreamlyCreator, "STREAMLY-CREATOR", "Creator Support", "STREAMLY", "US", "Fictional creator support campaign."),
        new(SeedIds.Campaigns.ShoporaAccess, "SHOPORA-ACCESS", "Account Access Support", "SHOPORA", "CO", "Fictional account access support campaign."),
        new(SeedIds.Campaigns.AeroLinkTravel, "AEROLINK-TRAVEL", "Travel Support", "AEROLINK", "CR", "Fictional travel support campaign.")
    ];

    public static readonly IReadOnlyList<UserScopeSeed> UserScopes =
    [
        UserScopeSeed.ForRegion(SeedIds.UserScopes.ManagerLatam, "manager.latam@opssphere.local", "LATAM"),
        UserScopeSeed.ForAccount(SeedIds.UserScopes.SupervisorNovabank, "supervisor.novabank@opssphere.local", "NOVABANK"),
        UserScopeSeed.ForCampaign(SeedIds.UserScopes.AgentNovabankCreditCard, "agent.novabank@opssphere.local", "NOVABANK-CC"),
        UserScopeSeed.ForRegion(SeedIds.UserScopes.ViewerLatam, "viewer.latam@opssphere.local", "LATAM")
    ];

    public sealed record RoleSeed(Guid Id, string Name, string Description);
    public sealed record PermissionSeed(Guid Id, string Code, string Name, string Description);
    public sealed record UserSeed(Guid Id, string Email, string FirstName, string LastName, string DisplayName, string RoleName);
    public sealed record RegionSeed(Guid Id, string Code, string Name);
    public sealed record CountrySeed(Guid Id, string Code, string Name, string RegionCode);
    public sealed record AccountSeed(Guid Id, string Code, string Name, string CountryCode, string Description);
    public sealed record CampaignSeed(Guid Id, string Code, string Name, string AccountCode, string CountryCode, string Description);
    public sealed record UserScopeSeed(Guid Id, string UserEmail, string ScopeType, string? RegionCode, string? CountryCode, string? AccountCode, string? CampaignCode)
    {
        public static UserScopeSeed ForRegion(Guid id, string userEmail, string regionCode) =>
            new(id, userEmail, "Region", regionCode, null, null, null);

        public static UserScopeSeed ForAccount(Guid id, string userEmail, string accountCode) =>
            new(id, userEmail, "Account", null, null, accountCode, null);

        public static UserScopeSeed ForCampaign(Guid id, string userEmail, string campaignCode) =>
            new(id, userEmail, "Campaign", null, null, null, campaignCode);
    }
}
