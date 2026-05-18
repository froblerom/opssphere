using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Features.DashboardManagement;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Enums;

namespace OpsSphere.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController(GetOperationalDashboardQueryHandler getOperationalDashboard) : ControllerBase
{
    [HttpGet("operational")]
    [Authorize(Policy = Permissions.DashboardView)]
    public async Task<IActionResult> GetOperationalDashboard(
        [FromQuery] Guid? regionId,
        [FromQuery] Guid? countryId,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? campaignId,
        [FromQuery] Guid? supervisorUserId,
        [FromQuery] Guid? agentUserId,
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] SlaState? slaState,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var result = await getOperationalDashboard.HandleAsync(
            new GetOperationalDashboardQuery(
                regionId,
                countryId,
                accountId,
                campaignId,
                supervisorUserId,
                agentUserId,
                status,
                priority,
                slaState,
                dateFrom,
                dateTo),
            cancellationToken);

        return Ok(new ApiResponse<OperationalDashboardDto>(result));
    }
}
