using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.CustomerManagement;
using OpsSphere.Application.Features.TicketManagement;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Services;
using OpsSphere.Infrastructure.Authorization;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.TicketManagement;

internal sealed class TicketRepository : ITicketRepository
{
    private readonly OpsSphereDbContext dbContext;
    private readonly ICurrentUserAuthorizationService authorizationService;
    private readonly SlaEvaluator slaEvaluator;

    public TicketRepository(
        OpsSphereDbContext dbContext,
        ICurrentUserAuthorizationService authorizationService,
        SlaEvaluator slaEvaluator)
    {
        this.dbContext = dbContext;
        this.authorizationService = authorizationService;
        this.slaEvaluator = slaEvaluator;
    }

    public async Task<CampaignTicketContextSnapshot?> GetCampaignSnapshotAsync(Guid campaignId, CancellationToken cancellationToken) =>
        await dbContext.Campaigns
            .AsNoTracking()
            .Where(c => c.Id == campaignId)
            .Select(c => new CampaignTicketContextSnapshot(
                c.Id,
                c.AccountId,
                c.CountryId,
                c.Country.RegionId,
                c.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CustomerTicketContextSnapshot?> GetCustomerSnapshotAsync(Guid customerId, CancellationToken cancellationToken) =>
        await dbContext.Customers
            .AsNoTracking()
            .Where(c => c.Id == customerId && !c.IsDeleted)
            .Select(c => new CustomerTicketContextSnapshot(
                c.Id,
                c.AccountId,
                c.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public Task AddTicketAsync(Ticket ticket, TicketStatusHistory statusHistory, TicketSlaState slaState, CancellationToken cancellationToken)
    {
        dbContext.Tickets.Add(ticket);
        dbContext.TicketStatusHistory.Add(statusHistory);
        dbContext.TicketSlaStates.Add(slaState);
        return Task.CompletedTask;
    }

    public async Task<Ticket?> GetTicketForAssignmentAsync(Guid ticketId, CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Tickets.Where(t => !t.IsDeleted))
            .Where(t => t.Id == ticketId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Ticket?> GetTicketForResolutionAsync(Guid ticketId, CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Tickets.Where(t => !t.IsDeleted))
            .Include(t => t.SlaStateRecord)
            .Include(t => t.Resolution)
            .Where(t => t.Id == ticketId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<EligibleAgentDto>> GetEligibleAgentsAsync(Guid campaignId, Guid accountId, CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserScopes)
                .ThenInclude(s => s.Account)
            .Include(u => u.UserScopes)
                .ThenInclude(s => s.Campaign)
            .Where(u =>
                u.IsActive &&
                u.UserRoles.Any(ur => ur.Role.Name == Roles.Agent && ur.Role.IsActive) &&
                u.UserScopes.Any(s =>
                    s.IsActive &&
                    ((s.ScopeType == ScopeTypes.Account && s.AccountId == accountId) ||
                     (s.ScopeType == ScopeTypes.Campaign && s.CampaignId == campaignId))))
            .OrderBy(u => u.DisplayName)
            .ThenBy(u => u.Id)
            .ToListAsync(cancellationToken);

        return users
            .Select(u =>
            {
                var scope = u.UserScopes
                    .Where(s =>
                        s.IsActive &&
                        ((s.ScopeType == ScopeTypes.Account && s.AccountId == accountId) ||
                         (s.ScopeType == ScopeTypes.Campaign && s.CampaignId == campaignId)))
                    .OrderBy(s => s.ScopeType == ScopeTypes.Campaign ? 0 : 1)
                    .First();

                return new EligibleAgentDto(
                    u.Id,
                    u.DisplayName,
                    scope.ScopeType,
                    scope.ScopeType == ScopeTypes.Campaign ? scope.Campaign?.Code : scope.Account?.Code);
            })
            .ToArray();
    }

    public async Task<AgentAssignmentCandidateSnapshot?> GetAgentAssignmentCandidateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserScopes)
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        return user is null
            ? null
            : new AgentAssignmentCandidateSnapshot(
                user.Id,
                user.IsActive,
                user.UserRoles.Any(ur => ur.Role.Name == Roles.Agent && ur.Role.IsActive),
                user.UserScopes
                    .Where(s => s.IsActive)
                    .Select(s => new AgentAssignmentScopeSnapshot(s.ScopeType, s.AccountId, s.CampaignId))
                    .ToArray());
    }

    public Task AddAssignmentAsync(TicketAssignment assignment, CancellationToken cancellationToken)
    {
        dbContext.TicketAssignments.Add(assignment);
        return Task.CompletedTask;
    }

    public Task AddStatusHistoryAsync(TicketStatusHistory statusHistory, CancellationToken cancellationToken)
    {
        dbContext.TicketStatusHistory.Add(statusHistory);
        return Task.CompletedTask;
    }

    public Task AddCommentAsync(TicketComment comment, CancellationToken cancellationToken)
    {
        dbContext.TicketComments.Add(comment);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<TicketCommentDto>> GetCommentsAsync(Guid ticketId, CancellationToken cancellationToken) =>
        await dbContext.TicketComments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Select(c => new TicketCommentDto(
                c.Id,
                c.TicketId,
                c.AuthorUserId,
                c.Author.DisplayName,
                c.Body,
                c.CreatedAt))
            .ToArrayAsync(cancellationToken);

    public Task AddEscalationAsync(TicketEscalation escalation, CancellationToken cancellationToken)
    {
        dbContext.TicketEscalations.Add(escalation);
        return Task.CompletedTask;
    }

    public Task AddResolutionAsync(TicketResolution resolution, CancellationToken cancellationToken)
    {
        dbContext.TicketResolutions.Add(resolution);
        return Task.CompletedTask;
    }

    public async Task DeactivateActiveEscalationsAsync(Guid ticketId, DateTime resolvedAt, CancellationToken cancellationToken)
    {
        var activeEscalations = await dbContext.TicketEscalations
            .Where(e => e.TicketId == ticketId && e.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var escalation in activeEscalations)
        {
            escalation.IsActive = false;
            escalation.ResolvedAt = resolvedAt;
        }
    }

    public async Task<IReadOnlyList<TicketStatusHistoryItemDto>> GetTicketStatusHistoryAsync(Guid ticketId, CancellationToken cancellationToken) =>
        await dbContext.TicketStatusHistory
            .AsNoTracking()
            .Where(h => h.TicketId == ticketId)
            .OrderBy(h => h.CreatedAt)
            .ThenBy(h => h.Id)
            .Select(h => new TicketStatusHistoryItemDto(
                h.Id, h.TicketId, h.PreviousStatus, h.NewStatus,
                h.ChangedByUserId, h.ChangeReason, h.CreatedAt))
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<CustomerTicketSummaryDto>> GetCustomerTicketHistoryAsync(Guid customerId, CancellationToken cancellationToken) =>
        (await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Tickets.Where(t => !t.IsDeleted && t.CustomerId == customerId))
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketSlaReadModel(
                t.Id,
                t.TicketNumber,
                null,
                null,
                null,
                t.Priority,
                t.Status,
                t.SlaState,
                t.SlaDueAt,
                t.IsEscalated,
                t.CreatedAt,
                t.AssignedAgentUserId,
                null,
                t.ResolvedAt,
                t.ClosedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.StartedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.DueAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.AtRiskThresholdPercent,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.CompletedAt))
            .ToArrayAsync(cancellationToken))
            .Select(t => new CustomerTicketSummaryDto(
                t.Id, t.TicketNumber,
                t.Status.ToString(), t.Priority.ToString(), EvaluateSlaState(t, DateTime.UtcNow).ToString(),
                t.CreatedAt, t.ResolvedAt, t.ClosedAt))
            .ToArray();

    public async Task<bool> HasActiveEscalationAsync(Guid ticketId, CancellationToken cancellationToken) =>
        await dbContext.TicketEscalations
            .AsNoTracking()
            .AnyAsync(e => e.TicketId == ticketId && e.IsActive, cancellationToken);

    public async Task<IReadOnlyList<EscalationQueueItemDto>> GetEscalationQueueAsync(CancellationToken cancellationToken)
    {
        var scopedTickets = ApplyScope(
            await GetProfileAsync(cancellationToken),
            dbContext.Tickets
                .AsNoTracking()
                .Where(t => !t.IsDeleted && t.Status == TicketStatus.Escalated && t.IsEscalated));

        var rows = await dbContext.TicketEscalations
            .AsNoTracking()
            .Where(e => e.IsActive)
            .Join(
                scopedTickets,
                escalation => escalation.TicketId,
                ticket => ticket.Id,
                (escalation, ticket) => new { escalation, ticket })
            .OrderByDescending(item => item.escalation.CreatedAt)
            .ThenBy(item => item.escalation.Id)
            .Select(item => new EscalationQueueReadModel(
                item.escalation.Id,
                item.ticket.Id,
                item.ticket.TicketNumber,
                item.ticket.Customer.FirstName + " " + item.ticket.Customer.LastName,
                item.ticket.Account.Name,
                item.ticket.Campaign.Name,
                item.ticket.Priority,
                item.ticket.Status,
                item.ticket.SlaState,
                item.ticket.SlaDueAt,
                item.escalation.CreatedAt,
                item.escalation.EscalatedByUserId,
                item.escalation.EscalatedByUser.DisplayName,
                item.escalation.EscalationReason,
                item.ticket.SlaStateRecord == null ? null : item.ticket.SlaStateRecord.StartedAt,
                item.ticket.SlaStateRecord == null ? null : item.ticket.SlaStateRecord.DueAt,
                item.ticket.SlaStateRecord == null ? null : item.ticket.SlaStateRecord.AtRiskThresholdPercent,
                item.ticket.SlaStateRecord == null ? null : item.ticket.SlaStateRecord.CompletedAt))
            .ToArrayAsync(cancellationToken);

        var evaluatedAt = DateTime.UtcNow;
        return rows
            .Select(item => new EscalationQueueItemDto(
                item.EscalationId,
                item.TicketId,
                item.TicketNumber,
                item.CustomerName,
                item.AccountName,
                item.CampaignName,
                item.Priority.ToString(),
                item.Status.ToString(),
                EvaluateSlaState(item, evaluatedAt).ToString(),
                item.EscalatedAt,
                item.EscalatedByUserId,
                item.EscalatedByName,
                item.EscalationReason))
            .ToArray();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UQ_Tickets_TicketNumber") == true)
        {
            throw new ConflictException("A ticket with this ticket number already exists. Please retry.");
        }
    }

    public async Task<TicketDetailDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken) =>
        (await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Tickets.AsNoTracking().Where(t => !t.IsDeleted))
            .Where(t => t.Id == id)
            .Select(t => new TicketDetailReadModel(
                t.Id,
                t.TicketNumber,
                t.CustomerId,
                t.Customer.FirstName + " " + t.Customer.LastName,
                t.AccountId,
                t.Account.Name,
                t.CampaignId,
                t.Campaign.Name,
                t.Category,
                t.Subject,
                t.Description,
                t.Priority.ToString(),
                t.Priority,
                t.Status,
                t.SlaState,
                t.SlaDueAt,
                t.IsEscalated,
                t.CreatedByUserId,
                t.CreatedAt,
                t.UpdatedAt,
                t.AssignedAgentUserId,
                t.AssignedAgentUser == null ? null : t.AssignedAgentUser.DisplayName,
                t.ResolvedAt,
                t.ClosedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.StartedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.DueAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.AtRiskThresholdPercent,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.CompletedAt))
            .FirstOrDefaultAsync(cancellationToken)) is { } ticket
            ? new TicketDetailDto(
                ticket.Id,
                ticket.TicketNumber,
                ticket.CustomerId,
                ticket.CustomerName,
                ticket.AccountId,
                ticket.AccountName,
                ticket.CampaignId,
                ticket.CampaignName,
                ticket.Category,
                ticket.Subject,
                ticket.Description,
                ticket.Priority.ToString(),
                ticket.Status.ToString(),
                EvaluateSlaState(ticket, DateTime.UtcNow).ToString(),
                ticket.SlaDueAt,
                ticket.IsEscalated,
                ticket.CreatedByUserId,
                ticket.CreatedAt,
                ticket.UpdatedAt,
                ticket.AssignedAgentUserId,
                ticket.AssignedAgentName,
                ticket.ResolvedAt,
                ticket.ClosedAt)
            : null;

    public async Task<IReadOnlyList<TicketListItemDto>> GetTicketsAsync(GetTicketsQuery query, CancellationToken cancellationToken)
    {
        var ticketsQuery = ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Tickets.AsNoTracking().Where(t => !t.IsDeleted));
        if (query.Status.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.Status == query.Status.Value);
        if (query.Priority.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.Priority == query.Priority.Value);
        if (query.RegionId.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.RegionId == query.RegionId.Value);
        if (query.CountryId.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.CountryId == query.CountryId.Value);
        if (query.AccountId.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.AccountId == query.AccountId.Value);
        if (query.CampaignId.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.CampaignId == query.CampaignId.Value);
        if (query.SupervisorUserId.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.SupervisorUserId == query.SupervisorUserId.Value);
        if (query.AssignedAgentUserId.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.AssignedAgentUserId == query.AssignedAgentUserId.Value);
        if (query.IsEscalated.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.IsEscalated == query.IsEscalated.Value);
        if (query.DateFrom.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.CreatedAt >= query.DateFrom.Value);
        if (query.DateTo.HasValue)
            ticketsQuery = ticketsQuery.Where(t => t.CreatedAt <= query.DateTo.Value);

        var ticketReadModels = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketSlaReadModel(
                t.Id,
                t.TicketNumber,
                t.Customer.FirstName + " " + t.Customer.LastName,
                t.Account.Name,
                t.Campaign.Name,
                t.Priority,
                t.Status,
                t.SlaState,
                t.SlaDueAt,
                t.IsEscalated,
                t.CreatedAt,
                t.AssignedAgentUserId,
                t.AssignedAgentUser == null ? null : t.AssignedAgentUser.DisplayName,
                t.ResolvedAt,
                t.ClosedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.StartedAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.DueAt,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.AtRiskThresholdPercent,
                t.SlaStateRecord == null ? null : t.SlaStateRecord.CompletedAt))
            .ToArrayAsync(cancellationToken);

        var evaluatedAt = DateTime.UtcNow;
        var evaluatedTickets = ticketReadModels
            .Select(t => new
            {
                Ticket = t,
                SlaState = EvaluateSlaState(t, evaluatedAt)
            });

        if (query.SlaState.HasValue)
            evaluatedTickets = evaluatedTickets.Where(t => t.SlaState == query.SlaState.Value);

        return evaluatedTickets
            .Select(t => new TicketListItemDto(
                t.Ticket.Id,
                t.Ticket.TicketNumber,
                t.Ticket.CustomerName ?? string.Empty,
                t.Ticket.AccountName ?? string.Empty,
                t.Ticket.CampaignName ?? string.Empty,
                t.Ticket.Priority.ToString(),
                t.Ticket.Status.ToString(),
                t.SlaState.ToString(),
                t.Ticket.IsEscalated,
                t.Ticket.CreatedAt,
                t.Ticket.AssignedAgentUserId,
                t.Ticket.AssignedAgentName))
            .ToArray();
    }

    public async Task<int> GetLatestSequenceForDatePrefixAsync(string datePrefix, CancellationToken cancellationToken)
    {
        var prefix = $"OPS-{datePrefix}-";
        var latest = await dbContext.Tickets
            .Where(t => t.TicketNumber.StartsWith(prefix))
            .Select(t => t.TicketNumber)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync(cancellationToken);

        if (latest is null) return 0;

        var suffix = latest[prefix.Length..];
        return int.TryParse(suffix, out var sequence) ? sequence : 0;
    }

    private async Task<CurrentUserAuthorizationProfile> GetProfileAsync(CancellationToken cancellationToken) =>
        await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken)
        ?? new CurrentUserAuthorizationProfile(Guid.Empty, string.Empty, false, [], [], []);

    private static IQueryable<Ticket> ApplyScope(CurrentUserAuthorizationProfile profile, IQueryable<Ticket> query) =>
        query.ApplyScopeFilter(profile);

    private SlaState EvaluateSlaState(ISlaReadModel ticket, DateTime evaluatedAt)
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

    private interface ISlaReadModel
    {
        TicketStatus Status { get; }
        SlaState StoredSlaState { get; }
        DateTime? SlaStartedAt { get; }
        DateTime? SlaDueAt { get; }
        int? SlaAtRiskThresholdPercent { get; }
        DateTime? SlaCompletedAt { get; }
    }

    private sealed record TicketSlaReadModel(
        Guid Id,
        string TicketNumber,
        string? CustomerName,
        string? AccountName,
        string? CampaignName,
        TicketPriority Priority,
        TicketStatus Status,
        SlaState StoredSlaState,
        DateTime? SlaDueAt,
        bool IsEscalated,
        DateTime CreatedAt,
        Guid? AssignedAgentUserId,
        string? AssignedAgentName,
        DateTime? ResolvedAt,
        DateTime? ClosedAt,
        DateTime? SlaStartedAt,
        DateTime? SlaRecordDueAt,
        int? SlaAtRiskThresholdPercent,
        DateTime? SlaCompletedAt) : ISlaReadModel
    {
        DateTime? ISlaReadModel.SlaDueAt => SlaRecordDueAt ?? SlaDueAt;
    }

    private sealed record TicketDetailReadModel(
        Guid Id,
        string TicketNumber,
        Guid CustomerId,
        string CustomerName,
        Guid AccountId,
        string AccountName,
        Guid CampaignId,
        string CampaignName,
        string Category,
        string Subject,
        string Description,
        string PriorityName,
        TicketPriority Priority,
        TicketStatus Status,
        SlaState StoredSlaState,
        DateTime? SlaDueAt,
        bool IsEscalated,
        Guid CreatedByUserId,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        Guid? AssignedAgentUserId,
        string? AssignedAgentName,
        DateTime? ResolvedAt,
        DateTime? ClosedAt,
        DateTime? SlaStartedAt,
        DateTime? SlaRecordDueAt,
        int? SlaAtRiskThresholdPercent,
        DateTime? SlaCompletedAt) : ISlaReadModel
    {
        DateTime? ISlaReadModel.SlaDueAt => SlaRecordDueAt ?? SlaDueAt;
    }

    private sealed record EscalationQueueReadModel(
        Guid EscalationId,
        Guid TicketId,
        string TicketNumber,
        string CustomerName,
        string AccountName,
        string CampaignName,
        TicketPriority Priority,
        TicketStatus Status,
        SlaState StoredSlaState,
        DateTime? SlaDueAt,
        DateTime EscalatedAt,
        Guid EscalatedByUserId,
        string EscalatedByName,
        string EscalationReason,
        DateTime? SlaStartedAt,
        DateTime? SlaRecordDueAt,
        int? SlaAtRiskThresholdPercent,
        DateTime? SlaCompletedAt) : ISlaReadModel
    {
        DateTime? ISlaReadModel.SlaDueAt => SlaRecordDueAt ?? SlaDueAt;
    }
}
