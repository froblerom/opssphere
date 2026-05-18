using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.TicketManagement;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Infrastructure.Authorization;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.TicketManagement;

internal sealed class TicketRepository : ITicketRepository
{
    private readonly OpsSphereDbContext dbContext;
    private readonly ICurrentUserAuthorizationService authorizationService;

    public TicketRepository(OpsSphereDbContext dbContext, ICurrentUserAuthorizationService authorizationService)
    {
        this.dbContext = dbContext;
        this.authorizationService = authorizationService;
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

        return await dbContext.TicketEscalations
            .AsNoTracking()
            .Where(e => e.IsActive)
            .Join(
                scopedTickets,
                escalation => escalation.TicketId,
                ticket => ticket.Id,
                (escalation, ticket) => new { escalation, ticket })
            .OrderByDescending(item => item.escalation.CreatedAt)
            .ThenBy(item => item.escalation.Id)
            .Select(item => new EscalationQueueItemDto(
                item.escalation.Id,
                item.ticket.Id,
                item.ticket.TicketNumber,
                item.ticket.Customer.FirstName + " " + item.ticket.Customer.LastName,
                item.ticket.Account.Name,
                item.ticket.Campaign.Name,
                item.ticket.Priority.ToString(),
                item.ticket.Status.ToString(),
                item.ticket.SlaState.ToString(),
                item.escalation.CreatedAt,
                item.escalation.EscalatedByUserId,
                item.escalation.EscalatedByUser.DisplayName,
                item.escalation.EscalationReason))
            .ToArrayAsync(cancellationToken);
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
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Tickets.AsNoTracking().Where(t => !t.IsDeleted))
            .Where(t => t.Id == id)
            .Select(t => new TicketDetailDto(
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
                t.Status.ToString(),
                t.SlaState.ToString(),
                t.SlaDueAt,
                t.IsEscalated,
                t.CreatedByUserId,
                t.CreatedAt,
                t.UpdatedAt,
                t.AssignedAgentUserId,
                t.AssignedAgentUser == null ? null : t.AssignedAgentUser.DisplayName))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<TicketListItemDto>> GetTicketsAsync(CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Tickets.AsNoTracking().Where(t => !t.IsDeleted))
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.TicketNumber,
                t.Customer.FirstName + " " + t.Customer.LastName,
                t.Account.Name,
                t.Campaign.Name,
                t.Priority.ToString(),
                t.Status.ToString(),
                t.SlaState.ToString(),
                t.IsEscalated,
                t.CreatedAt,
                t.AssignedAgentUserId,
                t.AssignedAgentUser == null ? null : t.AssignedAgentUser.DisplayName))
            .ToArrayAsync(cancellationToken);

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
}
