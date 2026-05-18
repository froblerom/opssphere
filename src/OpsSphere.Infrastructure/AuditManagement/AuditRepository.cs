using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.AuditManagement;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Authorization;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.AuditManagement;

internal sealed class AuditRepository : IAuditRepository
{
    private const string TicketEntityType = "Ticket";
    private const string CustomerEntityType = "Customer";
    private const string AccountEntityType = "Account";
    private const string CampaignEntityType = "Campaign";

    private readonly OpsSphereDbContext dbContext;
    private readonly ICurrentUserAuthorizationService authorizationService;

    public AuditRepository(OpsSphereDbContext dbContext, ICurrentUserAuthorizationService authorizationService)
    {
        this.dbContext = dbContext;
        this.authorizationService = authorizationService;
    }

    public async Task<PagedResultDto<AuditLogListItemDto>> GetAuditLogsAsync(GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        var profile = await GetProfileAsync(cancellationToken);
        var auditLogs = ApplyFilters(ApplyScope(profile), query);

        return await ToPagedListAsync(auditLogs, query.Page, query.PageSize, cancellationToken);
    }

    public async Task<AuditLogDetailDto?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var profile = await GetProfileAsync(cancellationToken);

        return await ApplyScope(profile)
            .Where(a => a.Id == id)
            .Select(a => new AuditLogDetailDto(
                a.Id,
                a.ActorUserId,
                a.ActorUser == null ? null : a.ActorUser.DisplayName,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.PreviousValue,
                a.NewValue,
                a.CorrelationId,
                a.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResultDto<AuditLogListItemDto>> GetEntityAuditHistoryAsync(GetEntityAuditHistoryQuery query, CancellationToken cancellationToken)
    {
        var profile = await GetProfileAsync(cancellationToken);
        var auditLogs = ApplyScope(profile)
            .Where(a => a.EntityType == query.EntityType.Trim() && a.EntityId == query.EntityId);

        if (query.FromUtc.HasValue)
            auditLogs = auditLogs.Where(a => a.CreatedAt >= query.FromUtc.Value);
        if (query.ToUtc.HasValue)
            auditLogs = auditLogs.Where(a => a.CreatedAt <= query.ToUtc.Value);

        return await ToPagedListAsync(auditLogs, query.Page, query.PageSize, cancellationToken);
    }

    private IQueryable<AuditLog> ApplyScope(CurrentUserAuthorizationProfile profile)
    {
        var auditLogs = dbContext.AuditLogs.AsNoTracking();
        if (profile.HasRole(Roles.Admin) && profile.HasPermission(Permissions.AuditAdminView))
            return auditLogs;

        var scopedTickets = dbContext.Tickets
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .ApplyScopeFilter(profile);
        var scopedCustomers = dbContext.Customers
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .ApplyScopeFilter(profile);

        return auditLogs.Where(a =>
            (a.EntityType == TicketEntityType && scopedTickets.Any(t => t.Id == a.EntityId)) ||
            (a.EntityType == CustomerEntityType && scopedCustomers.Any(c => c.Id == a.EntityId)));
    }

    private IQueryable<AuditLog> ApplyFilters(IQueryable<AuditLog> auditLogs, GetAuditLogsQuery query)
    {
        if (query.ActorUserId.HasValue)
            auditLogs = auditLogs.Where(a => a.ActorUserId == query.ActorUserId.Value);

        var action = query.Action?.Trim();
        if (!string.IsNullOrWhiteSpace(action))
            auditLogs = auditLogs.Where(a => a.Action == action);

        var entityType = query.EntityType?.Trim();
        if (!string.IsNullOrWhiteSpace(entityType))
            auditLogs = auditLogs.Where(a => a.EntityType == entityType);

        if (query.EntityId.HasValue)
            auditLogs = auditLogs.Where(a => a.EntityId == query.EntityId.Value);

        if (query.FromUtc.HasValue)
            auditLogs = auditLogs.Where(a => a.CreatedAt >= query.FromUtc.Value);

        if (query.ToUtc.HasValue)
            auditLogs = auditLogs.Where(a => a.CreatedAt <= query.ToUtc.Value);

        if (query.AccountId.HasValue)
            auditLogs = ApplyAccountFilter(auditLogs, query.AccountId.Value);

        if (query.CampaignId.HasValue)
            auditLogs = ApplyCampaignFilter(auditLogs, query.CampaignId.Value);

        return auditLogs;
    }

    private IQueryable<AuditLog> ApplyAccountFilter(IQueryable<AuditLog> auditLogs, Guid accountId) =>
        auditLogs.Where(a =>
            (a.EntityType == TicketEntityType && dbContext.Tickets.AsNoTracking().Any(t => !t.IsDeleted && t.Id == a.EntityId && t.AccountId == accountId)) ||
            (a.EntityType == CustomerEntityType && dbContext.Customers.AsNoTracking().Any(c => !c.IsDeleted && c.Id == a.EntityId && c.AccountId == accountId)) ||
            (a.EntityType == AccountEntityType && a.EntityId == accountId));

    private IQueryable<AuditLog> ApplyCampaignFilter(IQueryable<AuditLog> auditLogs, Guid campaignId) =>
        auditLogs.Where(a =>
            (a.EntityType == TicketEntityType && dbContext.Tickets.AsNoTracking().Any(t => !t.IsDeleted && t.Id == a.EntityId && t.CampaignId == campaignId)) ||
            (a.EntityType == CustomerEntityType && dbContext.Customers.AsNoTracking().Any(c => !c.IsDeleted && c.Id == a.EntityId && c.Account.Campaigns.Any(camp => camp.Id == campaignId))) ||
            (a.EntityType == CampaignEntityType && a.EntityId == campaignId));

    private static async Task<PagedResultDto<AuditLogListItemDto>> ToPagedListAsync(
        IQueryable<AuditLog> auditLogs,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await auditLogs.CountAsync(cancellationToken);
        var items = await auditLogs
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogListItemDto(
                a.Id,
                a.ActorUserId,
                a.ActorUser == null ? null : a.ActorUser.DisplayName,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.CreatedAt,
                a.CorrelationId))
            .ToArrayAsync(cancellationToken);

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResultDto<AuditLogListItemDto>(items, page, pageSize, totalCount, totalPages);
    }

    private async Task<CurrentUserAuthorizationProfile> GetProfileAsync(CancellationToken cancellationToken) =>
        await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken)
        ?? new CurrentUserAuthorizationProfile(Guid.Empty, string.Empty, false, [], [], []);
}
