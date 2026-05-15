namespace OpsSphere.Application.Common.Interfaces;

public interface IAuditWriter
{
    Task WriteAsync(
        string action,
        string entityType,
        Guid entityId,
        string? previousValue,
        string? newValue,
        CancellationToken cancellationToken);
}
