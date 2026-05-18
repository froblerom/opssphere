namespace OpsSphere.Application.Features.AuditManagement;

public sealed record AuditLogListItemDto(
    Guid Id,
    Guid? ActorUserId,
    string? ActorDisplayName,
    string Action,
    string EntityType,
    Guid EntityId,
    DateTime CreatedAt,
    string? CorrelationId);

public sealed record AuditLogDetailDto(
    Guid Id,
    Guid? ActorUserId,
    string? ActorDisplayName,
    string Action,
    string EntityType,
    Guid EntityId,
    string? PreviousValue,
    string? NewValue,
    string? CorrelationId,
    DateTime CreatedAt);

public sealed record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
