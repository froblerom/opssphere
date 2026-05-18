using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.DashboardManagement;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Services;
using OpsSphere.Infrastructure.Authorization;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.DashboardManagement;

internal sealed class DashboardRepository(
    OpsSphereDbContext dbContext,
    ICurrentUserAuthorizationService authorizationService,
    SlaEvaluator slaEvaluator,
    ICorrelationIdAccessor correlationIdAccessor,
    ILogger<DashboardRepository> logger) : IDashboardRepository
{
    private const long SlowQueryThresholdMilliseconds = 500;

    public async Task<OperationalDashboardDto> GetOperationalDashboardAsync(
        GetOperationalDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var profile = await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken)
            ?? new CurrentUserAuthorizationProfile(Guid.Empty, string.Empty, false, [], [], []);

        var ticketsQuery = dbContext.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .ApplyScopeFilter(profile);

        ticketsQuery = ApplyFilters(ticketsQuery, query);

        var ticketRows = await ticketsQuery
            .Select(t => new DashboardTicketRow(
                t.Id,
                t.RegionId,
                t.CountryId,
                t.AccountId,
                t.Account.Name,
                t.CampaignId,
                t.Campaign.Name,
                t.SupervisorUserId,
                t.SupervisorUser == null ? null : t.SupervisorUser.DisplayName,
                t.AssignedAgentUserId,
                t.AssignedAgentUser == null ? null : t.AssignedAgentUser.DisplayName,
                t.Status,
                t.Priority,
                t.SlaState,
                t.IsEscalated,
                t.CreatedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.StartedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.DueAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.AtRiskThresholdPercent,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.CompletedAt))
            .ToArrayAsync(cancellationToken);

        var evaluatedAt = DateTime.UtcNow;
        var evaluatedRows = ticketRows
            .Select(row => new EvaluatedDashboardTicketRow(row, EvaluateSlaState(row, evaluatedAt)))
            .Where(row => !query.SlaState.HasValue || row.SlaState == query.SlaState.Value)
            .ToArray();

        var result = new OperationalDashboardDto(
            evaluatedAt,
            evaluatedRows.Length,
            evaluatedRows.Count(row => row.Ticket.Status == TicketStatus.Open),
            evaluatedRows.Count(row => row.Ticket.AssignedAgentUserId.HasValue),
            evaluatedRows.Count(row => row.Ticket.IsEscalated),
            evaluatedRows.Count(row => row.SlaState == SlaState.Breached),
            evaluatedRows.Count(row => row.SlaState == SlaState.AtRisk),
            BuildStatusGroups(evaluatedRows, query),
            BuildPriorityGroups(evaluatedRows, query),
            BuildSlaGroups(evaluatedRows, query),
            BuildAccountGroups(evaluatedRows, query),
            BuildCampaignGroups(evaluatedRows, query),
            BuildAssignedAgentGroups(evaluatedRows, query),
            BuildSupervisorGroups(evaluatedRows, query),
            new DashboardAppliedFiltersDto(
                query.RegionId,
                query.CountryId,
                query.AccountId,
                query.CampaignId,
                query.SupervisorUserId,
                query.AgentUserId,
                query.Status?.ToString(),
                query.Priority?.ToString(),
                query.SlaState?.ToString(),
                query.DateFrom,
                query.DateTo));

        stopwatch.Stop();
        LogIfSlow(stopwatch.ElapsedMilliseconds, profile, query);

        return result;
    }

    private static IQueryable<Ticket> ApplyFilters(IQueryable<Ticket> query, GetOperationalDashboardQuery filters)
    {
        if (filters.RegionId.HasValue)
            query = query.Where(t => t.RegionId == filters.RegionId.Value);
        if (filters.CountryId.HasValue)
            query = query.Where(t => t.CountryId == filters.CountryId.Value);
        if (filters.AccountId.HasValue)
            query = query.Where(t => t.AccountId == filters.AccountId.Value);
        if (filters.CampaignId.HasValue)
            query = query.Where(t => t.CampaignId == filters.CampaignId.Value);
        if (filters.SupervisorUserId.HasValue)
            query = query.Where(t => t.SupervisorUserId == filters.SupervisorUserId.Value);
        if (filters.AgentUserId.HasValue)
            query = query.Where(t => t.AssignedAgentUserId == filters.AgentUserId.Value);
        if (filters.Status.HasValue)
            query = query.Where(t => t.Status == filters.Status.Value);
        if (filters.Priority.HasValue)
            query = query.Where(t => t.Priority == filters.Priority.Value);
        if (filters.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filters.DateTo.Value);

        return query;
    }

    private static IReadOnlyList<DashboardGroupItemDto> BuildStatusGroups(
        IReadOnlyList<EvaluatedDashboardTicketRow> rows,
        GetOperationalDashboardQuery query) =>
        rows.GroupBy(row => row.Ticket.Status)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.ToString())
            .Select(group => new DashboardGroupItemDto(
                group.Key.ToString(),
                group.Key.ToString(),
                group.Count(),
                group.Key.ToString(),
                null,
                null,
                null,
                query.DateFrom,
                query.DateTo))
            .ToArray();

    private static IReadOnlyList<DashboardGroupItemDto> BuildPriorityGroups(
        IReadOnlyList<EvaluatedDashboardTicketRow> rows,
        GetOperationalDashboardQuery query) =>
        rows.GroupBy(row => row.Ticket.Priority)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.ToString())
            .Select(group => new DashboardGroupItemDto(
                group.Key.ToString(),
                group.Key.ToString(),
                group.Count(),
                null,
                group.Key.ToString(),
                null,
                null,
                query.DateFrom,
                query.DateTo))
            .ToArray();

    private static IReadOnlyList<DashboardGroupItemDto> BuildSlaGroups(
        IReadOnlyList<EvaluatedDashboardTicketRow> rows,
        GetOperationalDashboardQuery query) =>
        rows.GroupBy(row => row.SlaState)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.ToString())
            .Select(group => new DashboardGroupItemDto(
                group.Key.ToString(),
                group.Key.ToString(),
                group.Count(),
                null,
                null,
                group.Key.ToString(),
                null,
                query.DateFrom,
                query.DateTo))
            .ToArray();

    private static IReadOnlyList<DashboardEntityGroupItemDto> BuildAccountGroups(
        IReadOnlyList<EvaluatedDashboardTicketRow> rows,
        GetOperationalDashboardQuery query) =>
        rows.GroupBy(row => new { row.Ticket.AccountId, row.Ticket.AccountName })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.AccountName)
            .Select(group => new DashboardEntityGroupItemDto(
                group.Key.AccountName,
                group.Key.AccountId,
                group.Count(),
                group.Key.AccountId,
                null,
                query.DateFrom,
                query.DateTo))
            .ToArray();

    private static IReadOnlyList<DashboardEntityGroupItemDto> BuildCampaignGroups(
        IReadOnlyList<EvaluatedDashboardTicketRow> rows,
        GetOperationalDashboardQuery query) =>
        rows.GroupBy(row => new { row.Ticket.CampaignId, row.Ticket.CampaignName, row.Ticket.AccountId })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.CampaignName)
            .Select(group => new DashboardEntityGroupItemDto(
                group.Key.CampaignName,
                group.Key.CampaignId,
                group.Count(),
                group.Key.AccountId,
                group.Key.CampaignId,
                query.DateFrom,
                query.DateTo))
            .ToArray();

    private static IReadOnlyList<DashboardUserGroupItemDto> BuildAssignedAgentGroups(
        IReadOnlyList<EvaluatedDashboardTicketRow> rows,
        GetOperationalDashboardQuery query) =>
        rows.GroupBy(row => new { row.Ticket.AssignedAgentUserId, row.Ticket.AssignedAgentName })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.AssignedAgentName ?? "Unassigned")
            .Select(group => new DashboardUserGroupItemDto(
                group.Key.AssignedAgentName ?? "Unassigned",
                group.Key.AssignedAgentUserId,
                group.Count(),
                group.Key.AssignedAgentUserId,
                null,
                query.DateFrom,
                query.DateTo))
            .ToArray();

    private static IReadOnlyList<DashboardUserGroupItemDto> BuildSupervisorGroups(
        IReadOnlyList<EvaluatedDashboardTicketRow> rows,
        GetOperationalDashboardQuery query) =>
        rows.Where(row => row.Ticket.SupervisorUserId.HasValue)
            .GroupBy(row => new { row.Ticket.SupervisorUserId, row.Ticket.SupervisorName })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.SupervisorName ?? "Unassigned")
            .Select(group => new DashboardUserGroupItemDto(
                group.Key.SupervisorName ?? "Unassigned",
                group.Key.SupervisorUserId,
                group.Count(),
                null,
                group.Key.SupervisorUserId,
                query.DateFrom,
                query.DateTo))
            .ToArray();

    private SlaState EvaluateSlaState(DashboardTicketRow ticket, DateTime evaluatedAt)
    {
        if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed || ticket.SlaCompletedAt.HasValue)
            return SlaState.Completed;

        if (!ticket.SlaStartedAt.HasValue || !ticket.SlaDueAt.HasValue)
            return ticket.StoredSlaState;

        return slaEvaluator.Evaluate(
            ticket.SlaStartedAt.Value,
            ticket.SlaDueAt.Value,
            ticket.SlaAtRiskThresholdPercent ?? 80,
            evaluatedAt);
    }

    private void LogIfSlow(long elapsedMilliseconds, CurrentUserAuthorizationProfile profile, GetOperationalDashboardQuery query)
    {
        if (elapsedMilliseconds <= SlowQueryThresholdMilliseconds)
            return;

        logger.LogWarning(
            "Slow dashboard query. CorrelationId={CorrelationId} UserId={UserId} Role={Role} ElapsedMilliseconds={ElapsedMilliseconds} ActiveFilterKeys={ActiveFilterKeys}",
            correlationIdAccessor.CorrelationId,
            profile.UserId,
            string.Join(",", profile.Roles),
            elapsedMilliseconds,
            GetActiveFilterKeys(query));
    }

    private static IReadOnlyList<string> GetActiveFilterKeys(GetOperationalDashboardQuery query)
    {
        var keys = new List<string>();
        if (query.RegionId.HasValue) keys.Add(nameof(query.RegionId));
        if (query.CountryId.HasValue) keys.Add(nameof(query.CountryId));
        if (query.AccountId.HasValue) keys.Add(nameof(query.AccountId));
        if (query.CampaignId.HasValue) keys.Add(nameof(query.CampaignId));
        if (query.SupervisorUserId.HasValue) keys.Add(nameof(query.SupervisorUserId));
        if (query.AgentUserId.HasValue) keys.Add(nameof(query.AgentUserId));
        if (query.Status.HasValue) keys.Add(nameof(query.Status));
        if (query.Priority.HasValue) keys.Add(nameof(query.Priority));
        if (query.SlaState.HasValue) keys.Add(nameof(query.SlaState));
        if (query.DateFrom.HasValue) keys.Add(nameof(query.DateFrom));
        if (query.DateTo.HasValue) keys.Add(nameof(query.DateTo));

        return keys;
    }

    private sealed record DashboardTicketRow(
        Guid Id,
        Guid RegionId,
        Guid CountryId,
        Guid AccountId,
        string AccountName,
        Guid CampaignId,
        string CampaignName,
        Guid? SupervisorUserId,
        string? SupervisorName,
        Guid? AssignedAgentUserId,
        string? AssignedAgentName,
        TicketStatus Status,
        TicketPriority Priority,
        SlaState StoredSlaState,
        bool IsEscalated,
        DateTime CreatedAt,
        DateTime? SlaStartedAt,
        DateTime? SlaDueAt,
        int? SlaAtRiskThresholdPercent,
        DateTime? SlaCompletedAt);

    private sealed record EvaluatedDashboardTicketRow(DashboardTicketRow Ticket, SlaState SlaState);
}
