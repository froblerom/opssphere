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
    private readonly UpdateTicketStatusCommandHandler updateTicketStatus;
    private readonly UpdateTicketPriorityCommandHandler updateTicketPriority;

    public TicketsController(
        ILogger<TicketsController> logger,
        CreateTicketCommandHandler createTicket,
        GetTicketsQueryHandler getTickets,
        GetTicketByIdQueryHandler getTicket,
        AssignTicketCommandHandler assignTicket,
        GetEligibleAgentsQueryHandler getEligibleAgents,
        UpdateTicketStatusCommandHandler updateTicketStatus,
        UpdateTicketPriorityCommandHandler updateTicketPriority)
    {
        this.logger = logger;
        this.createTicket = createTicket;
        this.getTickets = getTickets;
        this.getTicket = getTicket;
        this.assignTicket = assignTicket;
        this.getEligibleAgents = getEligibleAgents;
        this.updateTicketStatus = updateTicketStatus;
        this.updateTicketPriority = updateTicketPriority;
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

    [HttpPut("tickets/{id:guid}/status")]
    [Authorize(Policy = Permissions.TicketsUpdateStatus)]
    public async Task<IActionResult> UpdateTicketStatus(Guid id, UpdateTicketStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await updateTicketStatus.HandleAsync(
            new UpdateTicketStatusCommand(id, request.Status, request.ChangeReason),
            cancellationToken);
        logger.LogInformation(
            "Ticket status updated. TicketId={TicketId} TicketNumber={TicketNumber} PreviousStatus={PreviousStatus} NewStatus={NewStatus}",
            result.TicketId, result.TicketNumber, result.PreviousStatus, result.NewStatus);
        return Ok(new ApiResponse<UpdateTicketStatusResponse>(result));
    }

    [HttpPut("tickets/{id:guid}/priority")]
    [Authorize(Policy = Permissions.TicketsUpdatePriority)]
    public async Task<IActionResult> UpdateTicketPriority(Guid id, UpdateTicketPriorityRequest request, CancellationToken cancellationToken)
    {
        var result = await updateTicketPriority.HandleAsync(
            new UpdateTicketPriorityCommand(id, request.Priority, request.ChangeReason),
            cancellationToken);
        logger.LogInformation(
            "Ticket priority updated. TicketId={TicketId} TicketNumber={TicketNumber} PreviousPriority={PreviousPriority} NewPriority={NewPriority}",
            result.TicketId, result.TicketNumber, result.PreviousPriority, result.NewPriority);
        return Ok(new ApiResponse<UpdateTicketPriorityResponse>(result));
    }

    public sealed record CreateTicketRequest(
        Guid CustomerId,
        Guid AccountId,
        Guid CampaignId,
        string? Category,
        string? Priority,
        string? Subject,
        string? Description);
}
