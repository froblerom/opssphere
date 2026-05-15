using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;
using OpsSphere.Domain.Services;

namespace OpsSphere.Domain.Tests.Workflow;

public sealed class TicketResolutionRulesTests
{
    [Fact]
    public void Resolve_WhenSummaryIsEmpty_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanResolve(TicketStatus.Open, ""));
    }

    [Fact]
    public void Resolve_WhenSummaryIsWhitespaceOnly_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanResolve(TicketStatus.Open, "   "));
    }

    [Fact]
    public void Resolve_WhenSummaryIsNull_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanResolve(TicketStatus.Open, null));
    }

    [Fact]
    public void Resolve_WhenTicketIsClosed_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanResolve(TicketStatus.Closed, "Valid summary"));
    }

    [Fact]
    public void Resolve_WhenTicketIsInValidResolvableStatus_ShouldAllowResolvedTransition()
    {
        var ex = Record.Exception(() =>
            TicketWorkflowRules.EnsureCanResolve(TicketStatus.Open, "Valid summary"));

        Assert.Null(ex);
        Assert.True(TicketWorkflowRules.IsValidTransition(TicketStatus.Open, TicketStatus.Resolved));
    }

    [Fact]
    public void Resolve_WhenSummaryHasLeadingTrailingWhitespace_ShouldTrimSummary()
    {
        const string summary = "  issue resolved by patch  ";

        TicketWorkflowRules.EnsureCanResolve(TicketStatus.Open, summary);
        var result = TicketWorkflowRules.NormalizeResolutionSummary(summary);

        Assert.Equal("issue resolved by patch", result);
    }
}
