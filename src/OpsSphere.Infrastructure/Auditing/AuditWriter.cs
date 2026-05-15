using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.Auditing;

internal sealed class AuditWriter : IAuditWriter
{
    private readonly OpsSphereDbContext dbContext;
    private readonly ICurrentUserContext currentUserContext;
    private readonly ICorrelationIdAccessor correlationIdAccessor;

    public AuditWriter(
        OpsSphereDbContext dbContext,
        ICurrentUserContext currentUserContext,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        this.dbContext = dbContext;
        this.currentUserContext = currentUserContext;
        this.correlationIdAccessor = correlationIdAccessor;
    }

    public Task WriteAsync(
        string action,
        string entityType,
        Guid entityId,
        string? previousValue,
        string? newValue,
        CancellationToken cancellationToken)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = currentUserContext.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            PreviousValue = previousValue,
            NewValue = newValue,
            CorrelationId = correlationIdAccessor.CorrelationId,
            CreatedAt = DateTime.UtcNow
        });

        return Task.CompletedTask;
    }
}
