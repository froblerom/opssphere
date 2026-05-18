using OpsSphere.Domain.Enums;

namespace OpsSphere.Application.Features.TicketManagement;

public sealed record CreateTicketCommand(
    Guid CustomerId,
    Guid AccountId,
    Guid CampaignId,
    string? Category,
    string? Priority,
    string? Subject,
    string? Description);

public sealed record GetTicketByIdQuery(Guid Id);

public sealed record GetTicketsQuery(
    SlaState? SlaState,
    TicketStatus? Status,
    TicketPriority? Priority,
    Guid? RegionId,
    Guid? CountryId,
    Guid? AccountId,
    Guid? CampaignId,
    Guid? SupervisorUserId,
    Guid? AssignedAgentUserId,
    bool? IsEscalated,
    DateTime? DateFrom,
    DateTime? DateTo);

public sealed record AssignTicketCommand(
    Guid TicketId,
    Guid TargetAgentUserId,
    string? ReassignmentReason);

public sealed record GetEligibleAgentsQuery(Guid TicketId);

public sealed record UpdateTicketStatusCommand(
    Guid TicketId,
    string? Status,
    string? ChangeReason);

public sealed record UpdateTicketPriorityCommand(
    Guid TicketId,
    string? Priority,
    string? ChangeReason);

public sealed record AddInternalCommentCommand(
    Guid TicketId,
    string? Body);

public sealed record GetTicketCommentsQuery(Guid TicketId);

public sealed record EscalateTicketCommand(
    Guid TicketId,
    string? EscalationReason);

public sealed record GetEscalationQueueQuery;

public sealed record ResolveTicketCommand(Guid TicketId, string? ResolutionSummary, string? ResolutionCode);
public sealed record CloseTicketCommand(Guid TicketId);
public sealed record GetTicketStatusHistoryQuery(Guid TicketId);
