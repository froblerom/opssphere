namespace OpsSphere.Application.Features.CustomerManagement;

public sealed record CustomerDto(
    Guid Id,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? ExternalReference,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CustomerTicketSummaryDto(
    Guid Id, string TicketNumber,
    string Status, string Priority, string SlaState,
    DateTime CreatedAt, DateTime? ResolvedAt, DateTime? ClosedAt);
