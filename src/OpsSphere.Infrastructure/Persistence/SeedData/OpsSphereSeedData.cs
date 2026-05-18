using OpsSphere.Domain.Enums;

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

    public static readonly IReadOnlyList<CustomerSeed> Customers =
    [
        new(SeedIds.Customers.NovaBankCustomer1, "NOVABANK", "Carlos", "Mendez", "carlos.mendez@fictional.test", "+52-55-1234-5678", "NB-001"),
        new(SeedIds.Customers.StreamlyCustomer1, "STREAMLY", "Emma", "Johnson", "emma.j@fictional.test", "+1-415-555-0101", "ST-001"),
        new(SeedIds.Customers.ShoporaCustomer1, "SHOPORA", "Valentina", "Cruz", "v.cruz@fictional.test", "+57-601-555-0201", "SH-001"),
        new(SeedIds.Customers.AeroLinkCustomer1, "AEROLINK", "Rafael", "Mora", "r.mora@fictional.test", "+506-2222-3333", "AL-001")
    ];

    public static readonly IReadOnlyList<SlaPolicySeed> SlaPolicies =
    [
        new(SeedIds.SlaPolicies.CriticalPriorityDefault, null, null, "Critical", 4, 80),
        new(SeedIds.SlaPolicies.HighPriorityDefault, null, null, "High", 8, 80),
        new(SeedIds.SlaPolicies.NormalPriorityDefault, null, null, "Normal", 24, 80),
        new(SeedIds.SlaPolicies.LowPriorityDefault, null, null, "Low", 48, 80)
    ];

    public static readonly IReadOnlyList<UserScopeSeed> UserScopes =
    [
        UserScopeSeed.ForRegion(SeedIds.UserScopes.ManagerLatam, "manager.latam@opssphere.local", "LATAM"),
        UserScopeSeed.ForAccount(SeedIds.UserScopes.SupervisorNovabank, "supervisor.novabank@opssphere.local", "NOVABANK"),
        UserScopeSeed.ForCampaign(SeedIds.UserScopes.AgentNovabankCreditCard, "agent.novabank@opssphere.local", "NOVABANK-CC"),
        UserScopeSeed.ForRegion(SeedIds.UserScopes.ViewerLatam, "viewer.latam@opssphere.local", "LATAM")
    ];

    public static readonly IReadOnlyList<TicketSeed> Tickets =
    [
        new(
            SeedIds.Tickets.NovaBankOpen,
            SeedIds.TicketSlaStates.NovaBankOpen,
            "OPS-000001",
            TicketStatus.Open,
            TicketPriority.Normal,
            SlaState.WithinSla,
            null,
            "Billing Question",
            "Clarify recent credit card fee",
            "Customer asked for clarification about a recent fictional credit card fee.",
            -2,
            20,
            false,
            null,
            null),
        new(
            SeedIds.Tickets.NovaBankAssigned,
            SeedIds.TicketSlaStates.NovaBankAssigned,
            "OPS-000002",
            TicketStatus.Assigned,
            TicketPriority.High,
            SlaState.AtRisk,
            SeedIds.SlaPolicies.HighPriorityDefault,
            "Payment Review",
            "Review pending payment adjustment",
            "Customer needs a fictional payment adjustment reviewed by the card support team.",
            -7,
            1,
            false,
            SeedIds.Users.AgentNovabank,
            SeedIds.Users.SupervisorNovabank),
        new(
            SeedIds.Tickets.NovaBankInProgress,
            SeedIds.TicketSlaStates.NovaBankInProgress,
            "OPS-000003",
            TicketStatus.InProgress,
            TicketPriority.Critical,
            SlaState.Breached,
            SeedIds.SlaPolicies.CriticalPriorityDefault,
            "Card Access",
            "Restore card portal access",
            "Customer cannot access a fictional card portal and needs agent follow-up.",
            -12,
            -2,
            false,
            SeedIds.Users.AgentNovabank,
            SeedIds.Users.SupervisorNovabank),
        new(
            SeedIds.Tickets.NovaBankEscalated,
            SeedIds.TicketSlaStates.NovaBankEscalated,
            "OPS-000004",
            TicketStatus.Escalated,
            TicketPriority.Critical,
            SlaState.Breached,
            SeedIds.SlaPolicies.CriticalPriorityDefault,
            "Dispute Escalation",
            "Escalated charge dispute",
            "Customer dispute requires supervisor review before the next update.",
            -16,
            -6,
            true,
            SeedIds.Users.AgentNovabank,
            SeedIds.Users.SupervisorNovabank),
        new(
            SeedIds.Tickets.NovaBankResolved,
            SeedIds.TicketSlaStates.NovaBankResolved,
            "OPS-000005",
            TicketStatus.Resolved,
            TicketPriority.High,
            SlaState.Completed,
            SeedIds.SlaPolicies.HighPriorityDefault,
            "Limit Update",
            "Resolved card limit update",
            "Fictional card limit update was reviewed and resolved.",
            -30,
            -22,
            false,
            SeedIds.Users.AgentNovabank,
            SeedIds.Users.SupervisorNovabank),
        new(
            SeedIds.Tickets.NovaBankClosed,
            SeedIds.TicketSlaStates.NovaBankClosed,
            "OPS-000006",
            TicketStatus.Closed,
            TicketPriority.Low,
            SlaState.Completed,
            SeedIds.SlaPolicies.LowPriorityDefault,
            "Statement Copy",
            "Closed statement copy request",
            "Customer requested a fictional statement copy and the case was closed.",
            -72,
            -48,
            false,
            SeedIds.Users.AgentNovabank,
            SeedIds.Users.SupervisorNovabank)
    ];

    public sealed record CustomerSeed(Guid Id, string AccountCode, string FirstName, string LastName, string? Email, string? PhoneNumber, string? ExternalReference);
    public sealed record TicketSeed(
        Guid Id,
        Guid SlaStateId,
        string TicketNumber,
        TicketStatus Status,
        TicketPriority Priority,
        SlaState SlaState,
        Guid? SlaPolicyId,
        string Category,
        string Subject,
        string Description,
        int CreatedHoursOffset,
        int SlaDueHoursOffset,
        bool IsEscalated,
        Guid? AssignedAgentUserId,
        Guid? SupervisorUserId);
    public sealed record RoleSeed(Guid Id, string Name, string Description);
    public sealed record PermissionSeed(Guid Id, string Code, string Name, string Description);
    public sealed record UserSeed(Guid Id, string Email, string FirstName, string LastName, string DisplayName, string RoleName);
    public sealed record RegionSeed(Guid Id, string Code, string Name);
    public sealed record CountrySeed(Guid Id, string Code, string Name, string RegionCode);
    public sealed record AccountSeed(Guid Id, string Code, string Name, string CountryCode, string Description);
    public sealed record CampaignSeed(Guid Id, string Code, string Name, string AccountCode, string CountryCode, string Description);
    public sealed record SlaPolicySeed(Guid Id, string? AccountCode, string? CampaignCode, string Priority, int TargetHours, int AtRiskThresholdPercent);
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
