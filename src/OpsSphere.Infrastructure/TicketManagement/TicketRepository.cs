using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.TicketManagement;
using OpsSphere.Domain.Entities;
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
                t.UpdatedAt))
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
                t.CreatedAt))
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
