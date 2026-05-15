using Microsoft.EntityFrameworkCore;
using OpsSphere.Application.Common.Authorization;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Application.Features.CustomerManagement;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Authorization;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.Infrastructure.CustomerManagement;

internal sealed class CustomerManagementRepository : ICustomerManagementRepository
{
    private readonly OpsSphereDbContext dbContext;
    private readonly ICurrentUserAuthorizationService authorizationService;

    public CustomerManagementRepository(OpsSphereDbContext dbContext, ICurrentUserAuthorizationService authorizationService)
    {
        this.dbContext = dbContext;
        this.authorizationService = authorizationService;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Customers.AsNoTracking().Where(c => !c.IsDeleted))
            .OrderBy(c => c.Account.Name).ThenBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Select(c => new CustomerDto(c.Id, c.AccountId, c.Account.Code, c.Account.Name, c.FirstName, c.LastName, c.Email, c.PhoneNumber, c.ExternalReference, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .ToArrayAsync(cancellationToken);

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await ApplyScope(await GetProfileAsync(cancellationToken), dbContext.Customers.AsNoTracking().Where(c => !c.IsDeleted))
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto(c.Id, c.AccountId, c.Account.Code, c.Account.Name, c.FirstName, c.LastName, c.Email, c.PhoneNumber, c.ExternalReference, c.IsActive, c.CreatedAt, c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CustomerSnapshot?> GetCustomerSnapshotAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Customers.AsNoTracking().Where(c => c.Id == id && !c.IsDeleted)
            .Select(c => new CustomerSnapshot(c.Id, c.AccountId, c.ExternalReference, c.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<CustomerAccountSnapshot?> GetAccountSnapshotAsync(Guid accountId, CancellationToken cancellationToken) =>
        await dbContext.Accounts.AsNoTracking().Where(a => a.Id == accountId)
            .Select(a => new CustomerAccountSnapshot(a.Id, a.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public Task AddCustomerAsync(CustomerCreateModel model, CancellationToken cancellationToken)
    {
        dbContext.Customers.Add(new Customer
        {
            Id = model.Id,
            AccountId = model.AccountId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            ExternalReference = model.ExternalReference,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    public async Task UpdateCustomerAsync(Guid id, Guid accountId, string firstName, string lastName, string? email, string? phoneNumber, string? externalReference, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FirstAsync(c => c.Id == id, cancellationToken);
        customer.AccountId = accountId;
        customer.FirstName = firstName;
        customer.LastName = lastName;
        customer.Email = email;
        customer.PhoneNumber = phoneNumber;
        customer.ExternalReference = externalReference;
        customer.UpdatedAt = DateTime.UtcNow;
    }

    public async Task DeactivateCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FirstAsync(c => c.Id == id, cancellationToken);
        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    private async Task<CurrentUserAuthorizationProfile> GetProfileAsync(CancellationToken cancellationToken) =>
        await authorizationService.GetCurrentUserAuthorizationAsync(cancellationToken)
        ?? new CurrentUserAuthorizationProfile(Guid.Empty, string.Empty, false, [], [], []);

    private static IQueryable<Customer> ApplyScope(CurrentUserAuthorizationProfile profile, IQueryable<Customer> query) =>
        query.ApplyScopeFilter(profile);
}
