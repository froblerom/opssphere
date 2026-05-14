namespace OpsSphere.Domain.Entities;

public sealed class Region
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Country> Countries { get; set; } = [];
    public ICollection<UserScope> UserScopes { get; set; } = [];
}
