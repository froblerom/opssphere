namespace OpsSphere.Domain.Entities;

public sealed class Customer
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ExternalReference { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Account Account { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = [];
}
