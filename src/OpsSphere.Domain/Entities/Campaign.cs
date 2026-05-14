namespace OpsSphere.Domain.Entities;

public sealed class Campaign
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Account Account { get; set; } = null!;
    public Country Country { get; set; } = null!;
    public ICollection<UserScope> UserScopes { get; set; } = [];
    public ICollection<SlaPolicy> SlaPolicies { get; set; } = [];
}
