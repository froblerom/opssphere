using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;

namespace OpsSphere.Domain.Services;

public static class TicketWorkflowRules
{
    private static readonly Dictionary<TicketStatus, HashSet<TicketStatus>> AllowedTransitions = new()
    {
        [TicketStatus.Open] = [TicketStatus.Assigned, TicketStatus.InProgress, TicketStatus.Escalated, TicketStatus.Resolved],
        [TicketStatus.Assigned] = [TicketStatus.InProgress, TicketStatus.WaitingForCustomer, TicketStatus.Escalated, TicketStatus.Resolved],
        [TicketStatus.InProgress] = [TicketStatus.WaitingForCustomer, TicketStatus.Escalated, TicketStatus.Resolved],
        [TicketStatus.WaitingForCustomer] = [TicketStatus.InProgress, TicketStatus.Escalated, TicketStatus.Resolved],
        [TicketStatus.Escalated] = [TicketStatus.InProgress, TicketStatus.WaitingForCustomer, TicketStatus.Resolved],
        [TicketStatus.Resolved] = [TicketStatus.Closed],
        [TicketStatus.Closed] = []
    };

    public static bool IsValidTransition(TicketStatus from, TicketStatus to)
    {
        if (from == to)
            return false;

        return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static void EnsureValidTransition(TicketStatus from, TicketStatus to)
    {
        if (!IsValidTransition(from, to))
            throw new TicketDomainException($"Cannot transition ticket from {from} to {to}.");
    }

    public static void EnsureCanModify(TicketStatus currentStatus)
    {
        if (currentStatus == TicketStatus.Closed)
            throw new TicketDomainException("Closed tickets cannot be modified.");
    }

    public static void EnsureCanAssign(TicketStatus currentStatus)
    {
        if (currentStatus == TicketStatus.Closed)
            throw new TicketDomainException("Cannot assign a closed ticket.");
    }

    public static void EnsureCanAddComment(TicketStatus currentStatus, string? body)
    {
        if (currentStatus == TicketStatus.Closed)
            throw new TicketDomainException("Cannot add a comment to a closed ticket.");

        if (string.IsNullOrWhiteSpace(body))
            throw new TicketDomainException("Comment body cannot be empty.");
    }

    public static string NormalizeCommentBody(string? body)
        => body!.Trim();

    public static void EnsureCanEscalate(TicketStatus currentStatus, string? reason)
    {
        if (currentStatus == TicketStatus.Closed)
            throw new TicketDomainException("Cannot escalate a closed ticket.");

        if (currentStatus == TicketStatus.Escalated)
            throw new TicketDomainException("Ticket is already escalated.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new TicketDomainException("Escalation reason cannot be empty.");
    }

    public static string NormalizeEscalationReason(string? reason)
        => reason!.Trim();

    public static void EnsureCanResolve(TicketStatus currentStatus, string? resolutionSummary)
    {
        if (currentStatus == TicketStatus.Closed)
            throw new TicketDomainException("Cannot resolve a closed ticket.");

        if (string.IsNullOrWhiteSpace(resolutionSummary))
            throw new TicketDomainException("Resolution summary cannot be empty.");
    }

    public static string NormalizeResolutionSummary(string? resolutionSummary)
        => resolutionSummary!.Trim();

    public static void EnsureCanClose(TicketStatus currentStatus)
    {
        if (currentStatus != TicketStatus.Resolved)
            throw new TicketDomainException("Only resolved tickets can be closed.");
    }

    public static bool IsClosed(TicketStatus status)
        => status == TicketStatus.Closed;
}
