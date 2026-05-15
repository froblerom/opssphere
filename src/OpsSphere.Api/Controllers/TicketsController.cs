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
    private readonly AssignTicketCommandHandler assignTicket;
    private readonly GetEligibleAgentsQueryHandler getEligibleAgents;

    public TicketsController(
        ILogger<TicketsController> logger,
        CreateTicketCommandHandler createTicket,
        GetTicketsQueryHandler getTickets,
        GetTicketByIdQueryHandler getTicket,
        AssignTicketCommandHandler assignTicket,
        GetEligibleAgentsQueryHandler getEligibleAgents)
    {
        this.logger = logger;
        this.createTicket = createTicket;
        this.getTickets = getTickets;
        this.getTicket = getTicket;
        this.assignTicket = assignTicket;
        this.getEligibleAgents = getEligibleAgents;
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

    [HttpPost("tickets/{id:guid}/assign")]
    [Authorize(Policy = Permissions.TicketsAssign)]
    public async Task<IActionResult> AssignTicket(Guid id, AssignTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await assignTicket.HandleAsync(
            new AssignTicketCommand(id, request.TargetAgentUserId, request.ReassignmentReason),
            cancellationToken);
        logger.LogInformation(
            "Ticket assigned. TicketId={TicketId} TicketNumber={TicketNumber} AssignedAgentUserId={AssignedAgentUserId} Status={Status}",
            result.TicketId, result.TicketNumber, result.AssignedAgentUserId, result.Status);
        return Ok(new ApiResponse<AssignTicketResponse>(result));
    }

    [HttpGet("tickets/{id:guid}/eligible-agents")]
    [Authorize(Policy = Permissions.TicketsAssign)]
    public async Task<IActionResult> GetEligibleAgents(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<EligibleAgentDto>>(
            await getEligibleAgents.HandleAsync(new GetEligibleAgentsQuery(id), cancellationToken)));

    public sealed record CreateTicketRequest(
        Guid CustomerId,
        Guid AccountId,
        Guid CampaignId,
        string? Category,
        string? Priority,
        string? Subject,
        string? Description);
}
