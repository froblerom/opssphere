using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Features.TicketManagement;
using OpsSphere.Domain.Authorization;

namespace OpsSphere.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly ILogger<TicketsController> logger;
    private readonly CreateTicketCommandHandler createTicket;
    private readonly GetTicketsQueryHandler getTickets;
    private readonly GetTicketByIdQueryHandler getTicket;

    public TicketsController(
        ILogger<TicketsController> logger,
        CreateTicketCommandHandler createTicket,
        GetTicketsQueryHandler getTickets,
        GetTicketByIdQueryHandler getTicket)
    {
        this.logger = logger;
        this.createTicket = createTicket;
        this.getTickets = getTickets;
        this.getTicket = getTicket;
    }

    [HttpPost("tickets")]
    [Authorize(Policy = Permissions.TicketsCreate)]
    public async Task<IActionResult> CreateTicket(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTicketCommand(
            request.CustomerId,
            request.AccountId,
            request.CampaignId,
            request.Category,
            request.Priority,
            request.Subject,
            request.Description);
        var result = await createTicket.HandleAsync(command, cancellationToken);
        logger.LogInformation(
            "Ticket created. TicketId={TicketId} TicketNumber={TicketNumber} CampaignId={CampaignId} Priority={Priority}",
            result.Id, result.TicketNumber, request.CampaignId, result.Priority);
        return CreatedAtAction(nameof(GetTicket), new { id = result.Id }, new ApiResponse<CreateTicketResult>(result));
    }

    [HttpGet("tickets")]
    [Authorize(Policy = Permissions.TicketsView)]
    public async Task<IActionResult> GetTickets(CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<TicketListItemDto>>(await getTickets.HandleAsync(new GetTicketsQuery(), cancellationToken)));

    [HttpGet("tickets/{id:guid}")]
    [Authorize(Policy = Permissions.TicketsView)]
    public async Task<IActionResult> GetTicket(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<TicketDetailDto>(await getTicket.HandleAsync(new GetTicketByIdQuery(id), cancellationToken)));

    public sealed record CreateTicketRequest(
        Guid CustomerId,
        Guid AccountId,
        Guid CampaignId,
        string? Category,
        string? Priority,
        string? Subject,
        string? Description);
}
