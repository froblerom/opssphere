using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;
using OpsSphere.Domain.Services;

namespace OpsSphere.Domain.Tests.Workflow;

public sealed class TicketClosureRulesTests
{
    [Fact]
    public void Close_WhenTicketIsOpen_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanClose(TicketStatus.Open));
    }

    [Fact]
    public void Close_WhenTicketIsInProgress_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanClose(TicketStatus.InProgress));
    }

    [Fact]
    public void Close_WhenTicketIsAssigned_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanClose(TicketStatus.Assigned));
    }

    [Fact]
    public void Close_WhenTicketIsEscalated_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanClose(TicketStatus.Escalated));
    }

    [Fact]
    public void Close_WhenTicketIsWaitingForCustomer_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanClose(TicketStatus.WaitingForCustomer));
    }

    [Fact]
    public void Close_WhenTicketIsResolved_ShouldAllowClosedTransition()
    {
        var ex = Record.Exception(() =>
            TicketWorkflowRules.EnsureCanClose(TicketStatus.Resolved));

        Assert.Null(ex);
        Assert.True(TicketWorkflowRules.IsValidTransition(TicketStatus.Resolved, TicketStatus.Closed));
    }

    [Fact]
    public void Close_WhenTicketIsAlreadyClosed_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanClose(TicketStatus.Closed));
    }
}
