using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.SlaManagement;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Services;
using OpsSphere.Infrastructure.Authorization;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.TicketManagement;

internal sealed class SlaSummaryRepository(
    OpsSphereDbContext dbContext,
    ICurrentUserAuthorizationService authorizationService,
    SlaEvaluator slaEvaluator) : ISlaSummaryRepository
{
    public async Task<SlaSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken)
            ?? new CurrentUserAuthorizationProfile(Guid.Empty, string.Empty, false, [], [], []);

        var tickets = await dbContext.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .ApplyScopeFilter(profile)
            .Select(t => new SlaSummaryTicket(
                t.Status,
                t.SlaState,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.StartedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.DueAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.AtRiskThresholdPercent,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.CompletedAt))
            .ToArrayAsync(cancellationToken);

        var evaluatedAt = DateTime.UtcNow;
        var states = tickets
            .Select(ticket => Evaluate(ticket, evaluatedAt))
            .ToArray();

        return new SlaSummaryDto(
            states.Count(state => state == SlaState.WithinSla),
            states.Count(state => state == SlaState.AtRisk),
            states.Count(state => state == SlaState.Breached),
            states.Count(state => state == SlaState.Completed));
    }

    private SlaState Evaluate(SlaSummaryTicket ticket, DateTime evaluatedAt)
    {
        if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed || ticket.CompletedAt.HasValue)
            return SlaState.Completed;

        if (!ticket.StartedAt.HasValue || !ticket.DueAt.HasValue)
            return ticket.StoredSlaState;

        return slaEvaluator.Evaluate(
            ticket.StartedAt.Value,
            ticket.DueAt.Value,
            ticket.AtRiskThresholdPercent ?? 80,
            evaluatedAt);
    }

    private sealed record SlaSummaryTicket(
        TicketStatus Status,
        SlaState StoredSlaState,
        DateTime? StartedAt,
        DateTime? DueAt,
        int? AtRiskThresholdPercent,
        DateTime? CompletedAt);
}
