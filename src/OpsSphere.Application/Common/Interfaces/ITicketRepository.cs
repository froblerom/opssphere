using OpsSphere.Application.Features.TicketManagement;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Application.Common.Interfaces;

public interface ITicketRepository
{
    Task<CampaignTicketContextSnapshot?> GetCampaignSnapshotAsync(Guid campaignId, CancellationToken cancellationToken);
    Task<CustomerTicketContextSnapshot?> GetCustomerSnapshotAsync(Guid customerId, CancellationToken cancellationToken);
    Task AddTicketAsync(Ticket ticket, TicketStatusHistory statusHistory, TicketSlaState slaState, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<TicketDetailDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TicketListItemDto>> GetTicketsAsync(CancellationToken cancellationToken);
    Task<int> GetLatestSequenceForDatePrefixAsync(string datePrefix, CancellationToken cancellationToken);
}
