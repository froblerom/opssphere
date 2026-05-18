using OpsSphere.Application.Features.AuditManagement;

namespace OpsSphere.Application.Common.Interfaces;

public interface IAuditRepository
{
    Task<PagedResultDto<AuditLogListItemDto>> GetAuditLogsAsync(GetAuditLogsQuery query, CancellationToken cancellationToken);
    Task<AuditLogDetailDto?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResultDto<AuditLogListItemDto>> GetEntityAuditHistoryAsync(GetEntityAuditHistoryQuery query, CancellationToken cancellationToken);
}
