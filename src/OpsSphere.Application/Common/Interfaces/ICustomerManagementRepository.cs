using OpsSphere.Application.Features.CustomerManagement;

namespace OpsSphere.Application.Common.Interfaces;

public interface ICustomerManagementRepository
{
    Task<IReadOnlyList<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<CustomerDto?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CustomerSnapshot?> GetCustomerSnapshotAsync(Guid id, CancellationToken cancellationToken);
    Task<CustomerAccountSnapshot?> GetAccountSnapshotAsync(Guid accountId, CancellationToken cancellationToken);
    Task AddCustomerAsync(CustomerCreateModel model, CancellationToken cancellationToken);
    Task UpdateCustomerAsync(Guid id, Guid accountId, string firstName, string lastName, string? email, string? phoneNumber, string? externalReference, CancellationToken cancellationToken);
    Task DeactivateCustomerAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record CustomerCreateModel(Guid Id, Guid AccountId, string FirstName, string LastName, string? Email, string? PhoneNumber, string? ExternalReference);
public sealed record CustomerSnapshot(Guid Id, Guid AccountId, string? ExternalReference, bool IsActive);
public sealed record CustomerAccountSnapshot(Guid Id, bool IsActive);
