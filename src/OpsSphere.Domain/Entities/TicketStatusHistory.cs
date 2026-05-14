namespace OpsSphere.Domain.Entities;

public sealed class TicketStatusHistory
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public Guid ChangedByUserId { get; set; }
    public string? ChangeReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}
