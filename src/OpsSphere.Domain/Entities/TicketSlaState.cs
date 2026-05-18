namespace OpsSphere.Domain.Entities;

public sealed class TicketSlaState
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid? SlaPolicyId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime DueAt { get; set; }
    public int AtRiskThresholdPercent { get; set; } = 80;
    public string State { get; set; } = string.Empty;
    public DateTime? LastEvaluatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FinalState { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public SlaPolicy? SlaPolicy { get; set; }
}
