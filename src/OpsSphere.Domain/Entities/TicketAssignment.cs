namespace OpsSphere.Domain.Entities;

public sealed class TicketAssignment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid? PreviousAgentUserId { get; set; }
    public Guid NewAgentUserId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public string? AssignmentReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User? PreviousAgentUser { get; set; }
    public User NewAgentUser { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}
