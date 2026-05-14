namespace OpsSphere.Domain.Entities;

public sealed class Ticket
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid RegionId { get; set; }
    public Guid CountryId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedAgentUserId { get; set; }
    public Guid? SupervisorUserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SlaState { get; set; } = string.Empty;
    public DateTime? SlaDueAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsEscalated { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public Region Region { get; set; } = null!;
    public Country Country { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public Campaign Campaign { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public User? AssignedAgentUser { get; set; }
    public User? SupervisorUser { get; set; }
    public ICollection<TicketComment> Comments { get; set; } = [];
    public ICollection<TicketAssignment> Assignments { get; set; } = [];
    public ICollection<TicketStatusHistory> StatusHistory { get; set; } = [];
    public ICollection<TicketEscalation> Escalations { get; set; } = [];
    public TicketResolution? Resolution { get; set; }
    public TicketSlaState? SlaStateRecord { get; set; }
}
