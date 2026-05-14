namespace OpsSphere.Domain.Entities;

public sealed class TicketResolution
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid ResolvedByUserId { get; set; }
    public string ResolutionSummary { get; set; } = string.Empty;
    public string? ResolutionCode { get; set; }
    public string FinalSlaState { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User ResolvedByUser { get; set; } = null!;
}
