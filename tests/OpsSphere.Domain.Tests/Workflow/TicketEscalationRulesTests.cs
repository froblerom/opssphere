using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;
using OpsSphere.Domain.Services;

namespace OpsSphere.Domain.Tests.Workflow;

public sealed class TicketEscalationRulesTests
{
    [Fact]
    public void Escalate_WhenReasonIsEmpty_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanEscalate(TicketStatus.Open, ""));
    }

    [Fact]
    public void Escalate_WhenReasonIsWhitespaceOnly_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanEscalate(TicketStatus.Open, "   "));
    }

    [Fact]
    public void Escalate_WhenReasonIsNull_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanEscalate(TicketStatus.Open, null));
    }

    [Fact]
    public void Escalate_WhenTicketIsClosed_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanEscalate(TicketStatus.Closed, "Valid reason"));
    }

    [Fact]
    public void Escalate_WhenTicketIsAlreadyEscalated_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanEscalate(TicketStatus.Escalated, "Valid reason"));
    }

    [Fact]
    public void Escalate_WhenTicketIsOpen_ShouldAllowEscalatedTransition()
    {
        var ex = Record.Exception(() =>
            TicketWorkflowRules.EnsureCanEscalate(TicketStatus.Open, "Valid reason"));

        Assert.Null(ex);
        Assert.True(TicketWorkflowRules.IsValidTransition(TicketStatus.Open, TicketStatus.Escalated));
    }

    [Fact]
    public void Escalate_WhenReasonHasLeadingTrailingWhitespace_ShouldTrimReason()
    {
        const string reason = "  urgent issue  ";

        TicketWorkflowRules.EnsureCanEscalate(TicketStatus.Open, reason);
        var result = TicketWorkflowRules.NormalizeEscalationReason(reason);

        Assert.Equal("urgent issue", result);
    }
}
