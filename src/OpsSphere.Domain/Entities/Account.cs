namespace OpsSphere.Domain.Entities;

public sealed class Account
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Country Country { get; set; } = null!;
    public ICollection<Campaign> Campaigns { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<UserScope> UserScopes { get; set; } = [];
    public ICollection<SlaPolicy> SlaPolicies { get; set; } = [];
}
