using OpsSphere.Domain.Enums;

namespace OpsSphere.Application.Common.Interfaces;

public interface ISlaRepository
{
    Task<SlaPolicyLookupResult?> GetMatchingPolicyAsync(
        Guid? accountId,
        Guid? campaignId,
        TicketPriority priority,
        CancellationToken cancellationToken);
}

public sealed record SlaPolicyLookupResult(
    Guid Id,
    int TargetHours,
    int AtRiskThresholdPercent);
