using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Application.Features.AuditManagement;

public sealed class GetAuditLogsQueryHandler(IAuditRepository repository)
{
    public Task<PagedResultDto<AuditLogListItemDto>> HandleAsync(GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        ValidatePage(query.Page, query.PageSize);
        ValidateDateRange(query.FromUtc, query.ToUtc);

        return repository.GetAuditLogsAsync(query, cancellationToken);
    }

    private static void ValidatePage(int page, int pageSize)
    {
        var failures = new List<ValidationFailure>();
        if (page < 1) failures.Add(new ValidationFailure("page", "Page must be 1 or greater."));
        if (pageSize is < 1 or > 100) failures.Add(new ValidationFailure("pageSize", "Page size must be between 1 and 100."));
        if (failures.Count > 0) throw new ValidationException(failures);
    }

    private static void ValidateDateRange(DateTime? fromUtc, DateTime? toUtc)
    {
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
            throw new ValidationException("fromUtc", "From date/time must be before or equal to to date/time.");
    }
}

public sealed class GetAuditLogByIdQueryHandler(IAuditRepository repository)
{
    public async Task<AuditLogDetailDto> HandleAsync(GetAuditLogByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.Id == Guid.Empty)
            throw new ValidationException("id", "Audit log id is required.");

        return await repository.GetAuditLogByIdAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException("Audit log", query.Id);
    }
}

public sealed class GetEntityAuditHistoryQueryHandler(IAuditRepository repository)
{
    public async Task<PagedResultDto<AuditLogListItemDto>> HandleAsync(GetEntityAuditHistoryQuery query, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        if (string.IsNullOrWhiteSpace(query.EntityType))
            failures.Add(new ValidationFailure("entityType", "Entity type is required."));
        if (query.EntityId == Guid.Empty)
            failures.Add(new ValidationFailure("entityId", "Entity id is required."));
        if (query.Page < 1)
            failures.Add(new ValidationFailure("page", "Page must be 1 or greater."));
        if (query.PageSize is < 1 or > 100)
            failures.Add(new ValidationFailure("pageSize", "Page size must be between 1 and 100."));
        if (query.FromUtc.HasValue && query.ToUtc.HasValue && query.FromUtc.Value > query.ToUtc.Value)
            failures.Add(new ValidationFailure("fromUtc", "From date/time must be before or equal to to date/time."));

        if (failures.Count > 0) throw new ValidationException(failures);

        var result = await repository.GetEntityAuditHistoryAsync(query, cancellationToken);
        if (result.TotalCount == 0)
            throw new NotFoundException($"{query.EntityType} audit history", query.EntityId);

        return result;
    }
}
