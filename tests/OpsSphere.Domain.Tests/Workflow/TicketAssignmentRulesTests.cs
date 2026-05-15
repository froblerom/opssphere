using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;
using OpsSphere.Domain.Services;

namespace OpsSphere.Domain.Tests.Workflow;

public sealed class TicketAssignmentRulesTests
{
    [Fact]
    public void Assign_WhenTicketIsClosed_ShouldThrow()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanAssign(TicketStatus.Closed));
    }

    [Fact]
    public void Assign_WhenTicketIsOpen_ShouldSucceed()
    {
        var ex = Record.Exception(() =>
            TicketWorkflowRules.EnsureCanAssign(TicketStatus.Open));

        Assert.Null(ex);
    }

    [Fact]
    public void Assign_WhenTicketIsAssigned_ShouldSucceed()
    {
        var ex = Record.Exception(() =>
            TicketWorkflowRules.EnsureCanAssign(TicketStatus.Assigned));

        Assert.Null(ex);
    }
}
