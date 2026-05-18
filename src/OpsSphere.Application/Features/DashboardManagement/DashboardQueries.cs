using OpsSphere.Domain.Enums;

namespace OpsSphere.Application.Features.DashboardManagement;

public sealed record GetOperationalDashboardQuery(
    Guid? RegionId,
    Guid? CountryId,
    Guid? AccountId,
    Guid? CampaignId,
    Guid? SupervisorUserId,
    Guid? AgentUserId,
    TicketStatus? Status,
    TicketPriority? Priority,
    SlaState? SlaState,
    DateTime? DateFrom,
    DateTime? DateTo);
