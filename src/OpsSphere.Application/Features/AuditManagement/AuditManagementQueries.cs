namespace OpsSphere.Application.Features.AuditManagement;

public sealed record GetAuditLogsQuery(
    Guid? ActorUserId,
    string? Action,
    string? EntityType,
    Guid? EntityId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    Guid? AccountId,
    Guid? CampaignId,
    int Page = 1,
    int PageSize = 25);

public sealed record GetAuditLogByIdQuery(Guid Id);

public sealed record GetEntityAuditHistoryQuery(
    string EntityType,
    Guid EntityId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page = 1,
    int PageSize = 25);
