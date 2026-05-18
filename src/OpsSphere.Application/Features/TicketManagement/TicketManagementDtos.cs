namespace OpsSphere.Application.Features.TicketManagement;

public sealed record CampaignTicketContextSnapshot(
    Guid CampaignId,
    Guid AccountId,
    Guid CountryId,
    Guid RegionId,
    bool IsActive);

public sealed record CustomerTicketContextSnapshot(
    Guid CustomerId,
    Guid AccountId,
    bool IsActive);

public sealed record AgentAssignmentScopeSnapshot(
    string ScopeType,
    Guid? AccountId,
    Guid? CampaignId);

public sealed record AgentAssignmentCandidateSnapshot(
    Guid UserId,
    bool IsActive,
    bool HasAgentRole,
    IReadOnlyList<AgentAssignmentScopeSnapshot> ActiveScopes);

public sealed record TicketListItemDto(
    Guid Id,
    string TicketNumber,
    string CustomerName,
    string AccountName,
    string CampaignName,
    string Priority,
    string Status,
    string SlaState,
    bool IsEscalated,
    DateTime CreatedAt,
    Guid? AssignedAgentUserId,
    string? AssignedAgentName);

public sealed record TicketDetailDto(
    Guid Id,
    string TicketNumber,
    Guid CustomerId,
    string CustomerName,
    Guid AccountId,
    string AccountName,
    Guid CampaignId,
    string CampaignName,
    string Category,
    string Subject,
    string Description,
    string Priority,
    string Status,
    string SlaState,
    DateTime? SlaDueAt,
    bool IsEscalated,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    Guid? AssignedAgentUserId,
    string? AssignedAgentName,
    DateTime? ResolvedAt,
    DateTime? ClosedAt);

public sealed record CreateTicketResult(
    Guid Id,
    string TicketNumber,
    string Status,
    string Priority,
    string SlaState,
    DateTime? SlaDueAt);

public sealed record AssignTicketRequest(
    Guid TargetAgentUserId,
    string? ReassignmentReason);

public sealed record AssignTicketResponse(
    Guid TicketId,
    string TicketNumber,
    Guid AssignedAgentUserId,
    Guid? PreviousAgentUserId,
    string Status,
    string Message);

public sealed record EligibleAgentDto(
    Guid UserId,
    string DisplayName,
    string? ScopeType,
    string? ScopeReference);

public sealed record UpdateTicketStatusRequest(
    string? Status,
    string? ChangeReason);

public sealed record UpdateTicketStatusResponse(
    Guid TicketId,
    string TicketNumber,
    string PreviousStatus,
    string NewStatus,
    string Message);

public sealed record UpdateTicketPriorityRequest(
    string? Priority,
    string? ChangeReason);

public sealed record UpdateTicketPriorityResponse(
    Guid TicketId,
    string TicketNumber,
    string PreviousPriority,
    string NewPriority,
    string Message);

public sealed record AddInternalCommentRequest(
    string? Body);

public sealed record AddInternalCommentResponse(
    Guid CommentId,
    Guid TicketId,
    string TicketNumber,
    Guid AuthorUserId,
    string AuthorDisplayName,
    string Body,
    DateTime CreatedAt,
    string Message);

public sealed record TicketCommentDto(
    Guid Id,
    Guid TicketId,
    Guid AuthorUserId,
    string AuthorDisplayName,
    string Body,
    DateTime CreatedAt);

public sealed record EscalateTicketRequest(
    string? EscalationReason);

public sealed record EscalateTicketResponse(
    Guid TicketId,
    string TicketNumber,
    Guid EscalationId,
    string PreviousStatus,
    string NewStatus,
    string Message);

public sealed record EscalationQueueItemDto(
    Guid EscalationId,
    Guid TicketId,
    string TicketNumber,
    string CustomerName,
    string AccountName,
    string CampaignName,
    string Priority,
    string Status,
    string SlaState,
    DateTime EscalatedAt,
    Guid EscalatedByUserId,
    string EscalatedByName,
    string EscalationReason);

public sealed record ResolveTicketRequest(string? ResolutionSummary, string? ResolutionCode);

public sealed record ResolveTicketResponse(
    Guid TicketId, string TicketNumber, Guid ResolutionId,
    string PreviousStatus, string NewStatus,
    string FinalSlaState, DateTime ResolvedAt, string Message);

public sealed record CloseTicketResponse(
    Guid TicketId, string TicketNumber,
    string PreviousStatus, string NewStatus,
    DateTime ClosedAt, string Message);

public sealed record TicketStatusHistoryItemDto(
    Guid Id, Guid TicketId,
    string? PreviousStatus, string NewStatus,
    Guid ChangedByUserId, string? ChangeReason,
    DateTime CreatedAt);
