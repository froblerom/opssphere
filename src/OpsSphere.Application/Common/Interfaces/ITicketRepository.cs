using OpsSphere.Application.Features.TicketManagement;
using OpsSphere.Domain.Entities;

namespace OpsSphere.Application.Common.Interfaces;

public interface ITicketRepository
{
    Task<CampaignTicketContextSnapshot?> GetCampaignSnapshotAsync(Guid campaignId, CancellationToken cancellationToken);
    Task<CustomerTicketContextSnapshot?> GetCustomerSnapshotAsync(Guid customerId, CancellationToken cancellationToken);
    Task AddTicketAsync(Ticket ticket, TicketStatusHistory statusHistory, TicketSlaState slaState, CancellationToken cancellationToken);
    Task<Ticket?> GetTicketForAssignmentAsync(Guid ticketId, CancellationToken cancellationToken);
    Task<IReadOnlyList<EligibleAgentDto>> GetEligibleAgentsAsync(Guid campaignId, Guid accountId, CancellationToken cancellationToken);
    Task<AgentAssignmentCandidateSnapshot?> GetAgentAssignmentCandidateAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAssignmentAsync(TicketAssignment assignment, CancellationToken cancellationToken);
    Task AddStatusHistoryAsync(TicketStatusHistory statusHistory, CancellationToken cancellationToken);
    Task AddCommentAsync(TicketComment comment, CancellationToken cancellationToken);
    Task<IReadOnlyList<TicketCommentDto>> GetCommentsAsync(Guid ticketId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<TicketDetailDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TicketListItemDto>> GetTicketsAsync(CancellationToken cancellationToken);
    Task<int> GetLatestSequenceForDatePrefixAsync(string datePrefix, CancellationToken cancellationToken);
}
