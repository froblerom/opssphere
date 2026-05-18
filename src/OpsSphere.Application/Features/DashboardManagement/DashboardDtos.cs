namespace OpsSphere.Application.Features.DashboardManagement;

public sealed record OperationalDashboardDto(
    DateTime GeneratedAtUtc,
    int TotalTicketCount,
    int OpenTicketCount,
    int AssignedTicketCount,
    int EscalatedTicketCount,
    int BreachedTicketCount,
    int AtRiskTicketCount,
    IReadOnlyList<DashboardGroupItemDto> TicketsByStatus,
    IReadOnlyList<DashboardGroupItemDto> TicketsByPriority,
    IReadOnlyList<DashboardGroupItemDto> TicketsBySlaState,
    IReadOnlyList<DashboardEntityGroupItemDto> TicketsByAccount,
    IReadOnlyList<DashboardEntityGroupItemDto> TicketsByCampaign,
    IReadOnlyList<DashboardUserGroupItemDto> TicketsByAssignedAgent,
    IReadOnlyList<DashboardUserGroupItemDto> TicketsBySupervisor,
    DashboardAppliedFiltersDto AppliedFilters);

public sealed record DashboardGroupItemDto(
    string Label,
    string Key,
    int Count,
    string? Status,
    string? Priority,
    string? SlaState,
    bool? IsEscalated,
    DateTime? DateFrom,
    DateTime? DateTo);

public sealed record DashboardEntityGroupItemDto(
    string Label,
    Guid EntityId,
    int Count,
    Guid? AccountId,
    Guid? CampaignId,
    DateTime? DateFrom,
    DateTime? DateTo);

public sealed record DashboardUserGroupItemDto(
    string Label,
    Guid? UserId,
    int Count,
    Guid? AssignedAgentUserId,
    Guid? SupervisorUserId,
    DateTime? DateFrom,
    DateTime? DateTo);

public sealed record DashboardAppliedFiltersDto(
    Guid? RegionId,
    Guid? CountryId,
    Guid? AccountId,
    Guid? CampaignId,
    Guid? SupervisorUserId,
    Guid? AgentUserId,
    string? Status,
    string? Priority,
    string? SlaState,
    DateTime? DateFrom,
    DateTime? DateTo);
