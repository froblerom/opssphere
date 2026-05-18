using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Features.AuditManagement;
using OpsSphere.Domain.Authorization;

namespace OpsSphere.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public sealed class AuditController : ControllerBase
{
    private readonly GetAuditLogsQueryHandler getAuditLogs;
    private readonly GetAuditLogByIdQueryHandler getAuditLogById;
    private readonly GetEntityAuditHistoryQueryHandler getEntityAuditHistory;

    public AuditController(
        GetAuditLogsQueryHandler getAuditLogs,
        GetAuditLogByIdQueryHandler getAuditLogById,
        GetEntityAuditHistoryQueryHandler getEntityAuditHistory)
    {
        this.getAuditLogs = getAuditLogs;
        this.getAuditLogById = getAuditLogById;
        this.getEntityAuditHistory = getEntityAuditHistory;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.AuditView)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] Guid? actorUserId,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? campaignId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await getAuditLogs.HandleAsync(
            new GetAuditLogsQuery(actorUserId, action, entityType, entityId, fromUtc, toUtc, accountId, campaignId, page, pageSize),
            cancellationToken);

        return Ok(ToResponse(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.AuditView)]
    public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<AuditLogDetailDto>(
            await getAuditLogById.HandleAsync(new GetAuditLogByIdQuery(id), cancellationToken)));

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [Authorize(Policy = Permissions.AuditView)]
    public async Task<IActionResult> GetEntityAuditHistory(
        string entityType,
        Guid entityId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await getEntityAuditHistory.HandleAsync(
            new GetEntityAuditHistoryQuery(entityType, entityId, fromUtc, toUtc, page, pageSize),
            cancellationToken);

        return Ok(ToResponse(result));
    }

    private static PagedApiResponse<T> ToResponse<T>(PagedResultDto<T> result) =>
        new(result.Items, result.Page, result.PageSize, result.TotalCount, result.TotalPages);
}
