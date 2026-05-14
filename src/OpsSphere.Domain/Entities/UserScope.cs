namespace OpsSphere.Domain.Entities;

public sealed class UserScope
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public Guid? RegionId { get; set; }
    public Guid? CountryId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? CampaignId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public User User { get; set; } = null!;
    public Region? Region { get; set; }
    public Country? Country { get; set; }
    public Account? Account { get; set; }
    public Campaign? Campaign { get; set; }
}
