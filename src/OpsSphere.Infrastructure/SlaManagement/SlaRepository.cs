using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Enums;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.SlaManagement;

internal sealed class SlaRepository(OpsSphereDbContext dbContext) : ISlaRepository
{
    public async Task<SlaPolicyLookupResult?> GetMatchingPolicyAsync(
        Guid? accountId,
        Guid? campaignId,
        TicketPriority priority,
        CancellationToken cancellationToken)
    {
        var priorityName = priority.ToString();

        return await dbContext.SlaPolicies
            .AsNoTracking()
            .Where(policy =>
                policy.IsActive &&
                policy.Priority == priorityName &&
                ((campaignId.HasValue && policy.CampaignId == campaignId.Value) ||
                 (accountId.HasValue && policy.CampaignId == null && policy.AccountId == accountId.Value) ||
                 (policy.CampaignId == null && policy.AccountId == null)))
            .OrderBy(policy => campaignId.HasValue && policy.CampaignId == campaignId.Value ? 0 :
                accountId.HasValue && policy.AccountId == accountId.Value ? 1 : 2)
            .ThenBy(policy => policy.CreatedAt)
            .Select(policy => new SlaPolicyLookupResult(
                policy.Id,
                policy.TargetHours,
                policy.AtRiskThresholdPercent))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
