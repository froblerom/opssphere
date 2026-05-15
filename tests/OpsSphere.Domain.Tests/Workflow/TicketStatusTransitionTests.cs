using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;
using OpsSphere.Domain.Services;

namespace OpsSphere.Domain.Tests.Workflow;

public sealed class TicketStatusTransitionTests
{
    // ── Valid transitions ────────────────────────────────────────────────────

    [Theory]
    [InlineData(TicketStatus.Open,              TicketStatus.Assigned)]           // ValidTransition_OpenToAssigned_IsAllowed
    [InlineData(TicketStatus.Open,              TicketStatus.InProgress)]         // ValidTransition_OpenToInProgress_IsAllowed
    [InlineData(TicketStatus.Open,              TicketStatus.Escalated)]          // ValidTransition_OpenToEscalated_IsAllowed
    [InlineData(TicketStatus.Open,              TicketStatus.Resolved)]           // ValidTransition_OpenToResolved_IsAllowed
    [InlineData(TicketStatus.Assigned,          TicketStatus.InProgress)]         // ValidTransition_AssignedToInProgress_IsAllowed
    [InlineData(TicketStatus.Assigned,          TicketStatus.WaitingForCustomer)] // ValidTransition_AssignedToWaitingForCustomer_IsAllowed
    [InlineData(TicketStatus.Assigned,          TicketStatus.Escalated)]          // ValidTransition_AssignedToEscalated_IsAllowed
    [InlineData(TicketStatus.Assigned,          TicketStatus.Resolved)]           // ValidTransition_AssignedToResolved_IsAllowed
    [InlineData(TicketStatus.InProgress,        TicketStatus.WaitingForCustomer)] // ValidTransition_InProgressToWaitingForCustomer_IsAllowed
    [InlineData(TicketStatus.InProgress,        TicketStatus.Escalated)]          // ValidTransition_InProgressToEscalated_IsAllowed
    [InlineData(TicketStatus.InProgress,        TicketStatus.Resolved)]           // ValidTransition_InProgressToResolved_IsAllowed
    [InlineData(TicketStatus.WaitingForCustomer, TicketStatus.InProgress)]        // ValidTransition_WaitingForCustomerToInProgress_IsAllowed
    [InlineData(TicketStatus.WaitingForCustomer, TicketStatus.Escalated)]         // ValidTransition_WaitingForCustomerToEscalated_IsAllowed
    [InlineData(TicketStatus.WaitingForCustomer, TicketStatus.Resolved)]          // ValidTransition_WaitingForCustomerToResolved_IsAllowed
    [InlineData(TicketStatus.Escalated,         TicketStatus.InProgress)]         // ValidTransition_EscalatedToInProgress_IsAllowed
    [InlineData(TicketStatus.Escalated,         TicketStatus.WaitingForCustomer)] // ValidTransition_EscalatedToWaitingForCustomer_IsAllowed
    [InlineData(TicketStatus.Escalated,         TicketStatus.Resolved)]           // ValidTransition_EscalatedToResolved_IsAllowed
    [InlineData(TicketStatus.Resolved,          TicketStatus.Closed)]             // ValidTransition_ResolvedToClosed_IsAllowed
    public void ValidTransition_IsAllowed(TicketStatus from, TicketStatus to)
    {
        var result = TicketWorkflowRules.IsValidTransition(from, to);

        Assert.True(result);
    }

    [Theory]
    [InlineData(TicketStatus.Open,              TicketStatus.Assigned)]
    [InlineData(TicketStatus.Open,              TicketStatus.InProgress)]
    [InlineData(TicketStatus.Resolved,          TicketStatus.Closed)]
    public void EnsureValidTransition_WhenTransitionIsValid_DoesNotThrow(TicketStatus from, TicketStatus to)
    {
        var ex = Record.Exception(() => TicketWorkflowRules.EnsureValidTransition(from, to));

        Assert.Null(ex);
    }

    // ── Invalid transitions ──────────────────────────────────────────────────

    [Theory]
    [InlineData(TicketStatus.Closed, TicketStatus.Open)]
    [InlineData(TicketStatus.Closed, TicketStatus.InProgress)]
    [InlineData(TicketStatus.Closed, TicketStatus.Resolved)]
    public void InvalidTransition_ClosedToAnyStatus_IsRejected(TicketStatus from, TicketStatus to)
    {
        var result = TicketWorkflowRules.IsValidTransition(from, to);

        Assert.False(result);
    }

    [Fact]
    public void InvalidTransition_OpenToClosed_IsRejected()
    {
        var result = TicketWorkflowRules.IsValidTransition(TicketStatus.Open, TicketStatus.Closed);

        Assert.False(result);
    }

    [Fact]
    public void InvalidTransition_OpenToWaitingForCustomer_IsRejected()
    {
        var result = TicketWorkflowRules.IsValidTransition(TicketStatus.Open, TicketStatus.WaitingForCustomer);

        Assert.False(result);
    }

    [Fact]
    public void InvalidTransition_ResolvedToOpen_IsRejected()
    {
        var result = TicketWorkflowRules.IsValidTransition(TicketStatus.Resolved, TicketStatus.Open);

        Assert.False(result);
    }

    [Fact]
    public void InvalidTransition_ResolvedToInProgress_IsRejected()
    {
        var result = TicketWorkflowRules.IsValidTransition(TicketStatus.Resolved, TicketStatus.InProgress);

        Assert.False(result);
    }

    [Theory]
    [InlineData(TicketStatus.Open)]
    [InlineData(TicketStatus.InProgress)]
    [InlineData(TicketStatus.Closed)]
    public void InvalidTransition_SameStatusToSameStatus_IsRejected(TicketStatus status)
    {
        var result = TicketWorkflowRules.IsValidTransition(status, status);

        Assert.False(result);
    }

    [Theory]
    [InlineData(TicketStatus.Closed, TicketStatus.Open)]
    [InlineData(TicketStatus.Open,   TicketStatus.Closed)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Open)]
    public void EnsureValidTransition_WhenTransitionIsInvalid_ThrowsTicketDomainException(TicketStatus from, TicketStatus to)
    {
        Assert.Throws<TicketDomainException>(() => TicketWorkflowRules.EnsureValidTransition(from, to));
    }
}
