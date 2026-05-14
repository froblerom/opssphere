namespace OpsSphere.Domain.Entities;

public sealed class TicketComment
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
    public User Author { get; set; } = null!;
}
