namespace OpsSphere.Domain.Entities;

public sealed class SlaPolicy
{
    public Guid Id { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? CampaignId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int TargetHours { get; set; }
    public int AtRiskThresholdPercent { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Account? Account { get; set; }
    public Campaign? Campaign { get; set; }
    public ICollection<TicketSlaState> TicketSlaStates { get; set; } = [];
}
