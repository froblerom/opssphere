using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Features.SlaManagement;
using OpsSphere.Domain.Authorization;

namespace OpsSphere.Api.Controllers;

[ApiController]
[Route("api/sla")]
[Authorize]
public sealed class SlaController(GetSlaSummaryQueryHandler getSlaSummary) : ControllerBase
{
    [HttpGet("summary")]
    [Authorize(Policy = Permissions.SlaView)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await getSlaSummary.HandleAsync(new GetSlaSummaryQuery(), cancellationToken);
        return Ok(new ApiResponse<SlaSummaryDto>(result));
    }
}
