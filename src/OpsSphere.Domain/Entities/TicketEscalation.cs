namespace OpsSphere.Domain.Entities;

public sealed class TicketEscalation
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid EscalatedByUserId { get; set; }
    public string EscalationReason { get; set; } = string.Empty;
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User EscalatedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}
