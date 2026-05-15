using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;
using OpsSphere.Domain.Services;

namespace OpsSphere.Domain.Tests.Workflow;

public sealed class TicketModificationRulesTests
{
    [Fact]
    public void EnsureCanModify_ClosedTicket_ThrowsDomainException()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureCanModify(TicketStatus.Closed));
    }

    [Fact]
    public void EnsureCanModify_OpenTicket_Succeeds()
    {
        var ex = Record.Exception(() =>
            TicketWorkflowRules.EnsureCanModify(TicketStatus.Open));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureValidTransition_InvalidTransition_ThrowsDomainException()
    {
        Assert.Throws<TicketDomainException>(() =>
            TicketWorkflowRules.EnsureValidTransition(TicketStatus.Open, TicketStatus.WaitingForCustomer));
    }
}
