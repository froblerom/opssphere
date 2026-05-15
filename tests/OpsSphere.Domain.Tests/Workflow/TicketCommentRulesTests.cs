using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;
using OpsSphere.Domain.Services;

namespace OpsSphere.Domain.Tests.Workflow;

public sealed class TicketCommentRulesTests
{
    [Fact]
    public void AddComment_WhenBodyIsEmpty_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanAddComment(TicketStatus.Open, ""));
    }

    [Fact]
    public void AddComment_WhenBodyIsWhitespaceOnly_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanAddComment(TicketStatus.Open, "   "));
    }

    [Fact]
    public void AddComment_WhenBodyIsNull_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanAddComment(TicketStatus.Open, null));
    }

    [Fact]
    public void AddComment_WhenTicketIsClosed_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanAddComment(TicketStatus.Closed, "Valid body"));
    }

    [Fact]
    public void AddComment_WhenBodyHasLeadingOrTrailingWhitespace_ShouldTrimContent()
    {
        const string body = "  trimmed comment  ";

        TicketWorkflowRules.EnsureCanAddComment(TicketStatus.Open, body);
        var result = TicketWorkflowRules.NormalizeCommentBody(body);

        Assert.Equal("trimmed comment", result);
    }

    [Fact]
    public void AddComment_WhenBodyIsValid_ShouldSucceed()
    {
        var ex = Record.Exception(() =>
            TicketWorkflowRules.EnsureCanAddComment(TicketStatus.Open, "A valid comment"));

        Assert.Null(ex);
    }
}
