using System.Text.Json;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Application.Features.CustomerManagement;

public sealed class GetCustomersQueryHandler(ICustomerManagementRepository repository)
{
    public Task<IReadOnlyList<CustomerDto>> HandleAsync(GetCustomersQuery query, CancellationToken cancellationToken) =>
        repository.GetCustomersAsync(cancellationToken);
}

public sealed class GetCustomerByIdQueryHandler(ICustomerManagementRepository repository)
{
    public async Task<CustomerDto> HandleAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken) =>
        await repository.GetCustomerByIdAsync(query.Id, cancellationToken) ?? throw new NotFoundException("Customer", query.Id);
}

public sealed class CreateCustomerCommandHandler(ICustomerManagementRepository repository, IAuditWriter auditWriter, IScopeAuthorizationService scopeAuthorization)
{
    public async Task<CustomerDto> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var account = await repository.GetAccountSnapshotAsync(command.AccountId, cancellationToken)
            ?? throw new ValidationException("accountId", "Account must exist.");
        if (!account.IsActive)
            throw new ValidationException("accountId", "Customer parent account must be active.");

        var access = await scopeAuthorization.CanAccessAccountAsync(command.AccountId, cancellationToken);
        if (!access.IsAllowed)
            throw new NotFoundException("Account", command.AccountId);

        var (firstName, lastName, email, phoneNumber, externalReference) = CustomerValidation.Normalize(
            command.FirstName, command.LastName, command.Email, command.PhoneNumber, command.ExternalReference);

        var id = Guid.NewGuid();
        await repository.AddCustomerAsync(new CustomerCreateModel(id, command.AccountId, firstName, lastName, email, phoneNumber, externalReference), cancellationToken);
        await auditWriter.WriteAsync("CustomerCreated", "Customer", id, null,
            CustomerAuditJson.Serialize(new { command.AccountId, externalReference, IsActive = true }),
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetCustomerByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Customer", id);
    }
}

public sealed class UpdateCustomerCommandHandler(ICustomerManagementRepository repository, IAuditWriter auditWriter, IScopeAuthorizationService scopeAuthorization)
{
    public async Task<CustomerDto> HandleAsync(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetCustomerSnapshotAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Customer", command.Id);

        var access = await scopeAuthorization.CanAccessAccountAsync(existing.AccountId, cancellationToken);
        if (!access.IsAllowed)
            throw new NotFoundException("Customer", command.Id);

        var account = await repository.GetAccountSnapshotAsync(command.AccountId, cancellationToken)
            ?? throw new ValidationException("accountId", "Account must exist.");
        if (!account.IsActive)
            throw new ValidationException("accountId", "Customer parent account must be active.");

        if (command.AccountId != existing.AccountId)
        {
            var newAccess = await scopeAuthorization.CanAccessAccountAsync(command.AccountId, cancellationToken);
            if (!newAccess.IsAllowed)
                throw new ValidationException("accountId", "Account is not accessible within your scope.");
        }

        var (firstName, lastName, email, phoneNumber, externalReference) = CustomerValidation.Normalize(
            command.FirstName, command.LastName, command.Email, command.PhoneNumber, command.ExternalReference);

        await repository.UpdateCustomerAsync(command.Id, command.AccountId, firstName, lastName, email, phoneNumber, externalReference, cancellationToken);
        await auditWriter.WriteAsync("CustomerUpdated", "Customer", command.Id,
            CustomerAuditJson.Serialize(new { existing.AccountId, existing.ExternalReference, existing.IsActive }),
            CustomerAuditJson.Serialize(new { command.AccountId, externalReference, existing.IsActive }),
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetCustomerByIdAsync(command.Id, cancellationToken) ?? throw new NotFoundException("Customer", command.Id);
    }
}

public sealed class DeactivateCustomerCommandHandler(ICustomerManagementRepository repository, IAuditWriter auditWriter, IScopeAuthorizationService scopeAuthorization)
{
    public async Task HandleAsync(DeactivateCustomerCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetCustomerSnapshotAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException("Customer", command.Id);

        var access = await scopeAuthorization.CanAccessAccountAsync(existing.AccountId, cancellationToken);
        if (!access.IsAllowed)
            throw new NotFoundException("Customer", command.Id);

        if (!existing.IsActive) return;

        await repository.DeactivateCustomerAsync(command.Id, cancellationToken);
        await auditWriter.WriteAsync("CustomerDeactivated", "Customer", command.Id,
            CustomerAuditJson.Serialize(new { existing.IsActive }),
            CustomerAuditJson.Serialize(new { IsActive = false }),
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetCustomerTicketsQueryHandler(ICustomerManagementRepository repository)
{
    public async Task<IReadOnlyList<CustomerTicketSummaryDto>> HandleAsync(GetCustomerTicketsQuery query, CancellationToken cancellationToken)
    {
        _ = await repository.GetCustomerByIdAsync(query.CustomerId, cancellationToken)
            ?? throw new NotFoundException("Customer", query.CustomerId);
        return [];
    }
}

internal static class CustomerValidation
{
    public static (string FirstName, string LastName, string? Email, string? PhoneNumber, string? ExternalReference) Normalize(
        string? firstName, string? lastName, string? email, string? phoneNumber, string? externalReference)
    {
        var failures = new List<ValidationFailure>();
        if (string.IsNullOrWhiteSpace(firstName)) failures.Add(new ValidationFailure("firstName", "First name is required."));
        else if (firstName.Trim().Length > 100) failures.Add(new ValidationFailure("firstName", "First name must be 100 characters or fewer."));
        if (string.IsNullOrWhiteSpace(lastName)) failures.Add(new ValidationFailure("lastName", "Last name is required."));
        else if (lastName.Trim().Length > 100) failures.Add(new ValidationFailure("lastName", "Last name must be 100 characters or fewer."));
        if (email is not null && email.Trim().Length > 256) failures.Add(new ValidationFailure("email", "Email must be 256 characters or fewer."));
        if (phoneNumber is not null && phoneNumber.Trim().Length > 50) failures.Add(new ValidationFailure("phoneNumber", "Phone number must be 50 characters or fewer."));
        if (externalReference is not null && externalReference.Trim().Length > 100) failures.Add(new ValidationFailure("externalReference", "External reference must be 100 characters or fewer."));
        if (failures.Count > 0) throw new ValidationException(failures);

        return (
            firstName!.Trim(),
            lastName!.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim());
    }
}

internal static class CustomerAuditJson
{
    public static string Serialize(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
}
