namespace OpsSphere.Domain.Enums;

public enum TicketStatus
{
    Open = 1,
    Assigned = 2,
    InProgress = 3,
    WaitingForCustomer = 4,
    Escalated = 5,
    Resolved = 6,
    Closed = 7
}
