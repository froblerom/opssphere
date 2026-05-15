namespace OpsSphere.Application.Features.CustomerManagement;

public sealed record GetCustomersQuery;
public sealed record GetCustomerByIdQuery(Guid Id);
public sealed record CreateCustomerCommand(Guid AccountId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? ExternalReference);
public sealed record UpdateCustomerCommand(Guid Id, Guid AccountId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? ExternalReference);
public sealed record DeactivateCustomerCommand(Guid Id);
public sealed record GetCustomerTicketsQuery(Guid CustomerId);
