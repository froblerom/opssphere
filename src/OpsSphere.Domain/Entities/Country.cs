namespace OpsSphere.Domain.Entities;

public sealed class Country
{
    public Guid Id { get; set; }
    public Guid RegionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Region Region { get; set; } = null!;
    public ICollection<Account> Accounts { get; set; } = [];
    public ICollection<Campaign> Campaigns { get; set; } = [];
    public ICollection<UserScope> UserScopes { get; set; } = [];
}
