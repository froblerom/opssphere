namespace OpsSphere.Application.Features.TicketManagement;

public sealed record CreateTicketCommand(
    Guid CustomerId,
    Guid AccountId,
    Guid CampaignId,
    string? Category,
    string? Priority,
    string? Subject,
    string? Description);

public sealed record GetTicketByIdQuery(Guid Id);

public sealed record GetTicketsQuery;
