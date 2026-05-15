namespace OpsSphere.Application.Features.TicketManagement;

public sealed record CampaignTicketContextSnapshot(
    Guid CampaignId,
    Guid AccountId,
    Guid CountryId,
    Guid RegionId,
    bool IsActive);

public sealed record CustomerTicketContextSnapshot(
    Guid CustomerId,
    Guid AccountId,
    bool IsActive);

public sealed record TicketListItemDto(
    Guid Id,
    string TicketNumber,
    string CustomerName,
    string AccountName,
    string CampaignName,
    string Priority,
    string Status,
    string SlaState,
    bool IsEscalated,
    DateTime CreatedAt);

public sealed record TicketDetailDto(
    Guid Id,
    string TicketNumber,
    Guid CustomerId,
    string CustomerName,
    Guid AccountId,
    string AccountName,
    Guid CampaignId,
    string CampaignName,
    string Category,
    string Subject,
    string Description,
    string Priority,
    string Status,
    string SlaState,
    DateTime? SlaDueAt,
    bool IsEscalated,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateTicketResult(
    Guid Id,
    string TicketNumber,
    string Status,
    string Priority,
    string SlaState,
    DateTime? SlaDueAt);
