using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Features.CustomerManagement;
using OpsSphere.Domain.Authorization;

namespace OpsSphere.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly ILogger<CustomersController> logger;
    private readonly GetCustomersQueryHandler getCustomers;
    private readonly GetCustomerByIdQueryHandler getCustomer;
    private readonly CreateCustomerCommandHandler createCustomer;
    private readonly UpdateCustomerCommandHandler updateCustomer;
    private readonly DeactivateCustomerCommandHandler deactivateCustomer;
    private readonly GetCustomerTicketsQueryHandler getCustomerTickets;

    public CustomersController(
        ILogger<CustomersController> logger,
        GetCustomersQueryHandler getCustomers,
        GetCustomerByIdQueryHandler getCustomer,
        CreateCustomerCommandHandler createCustomer,
        UpdateCustomerCommandHandler updateCustomer,
        DeactivateCustomerCommandHandler deactivateCustomer,
        GetCustomerTicketsQueryHandler getCustomerTickets)
    {
        this.logger = logger;
        this.getCustomers = getCustomers;
        this.getCustomer = getCustomer;
        this.createCustomer = createCustomer;
        this.updateCustomer = updateCustomer;
        this.deactivateCustomer = deactivateCustomer;
        this.getCustomerTickets = getCustomerTickets;
    }

    [HttpGet("customers")]
    [Authorize(Policy = Permissions.CustomersView)]
    public async Task<IActionResult> GetCustomers(CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<CustomerDto>>(await getCustomers.HandleAsync(new GetCustomersQuery(), cancellationToken)));

    [HttpGet("customers/{id:guid}")]
    [Authorize(Policy = Permissions.CustomersView)]
    public async Task<IActionResult> GetCustomer(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<CustomerDto>(await getCustomer.HandleAsync(new GetCustomerByIdQuery(id), cancellationToken)));

    [HttpPost("customers")]
    [Authorize(Policy = Permissions.CustomersCreate)]
    public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await createCustomer.HandleAsync(
            new CreateCustomerCommand(request.AccountId, request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.ExternalReference),
            cancellationToken);
        logger.LogInformation("Customer created. CustomerId={CustomerId} AccountId={AccountId}", result.Id, result.AccountId);
        return CreatedAtAction(nameof(GetCustomer), new { id = result.Id }, new ApiResponse<CustomerDto>(result));
    }

    [HttpPut("customers/{id:guid}")]
    [Authorize(Policy = Permissions.CustomersUpdate)]
    public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await updateCustomer.HandleAsync(
            new UpdateCustomerCommand(id, request.AccountId, request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.ExternalReference),
            cancellationToken);
        logger.LogInformation("Customer updated. CustomerId={CustomerId} AccountId={AccountId}", result.Id, result.AccountId);
        return Ok(new ApiResponse<CustomerDto>(result));
    }

    [HttpPost("customers/{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.CustomersUpdate)]
    public async Task<IActionResult> DeactivateCustomer(Guid id, CancellationToken cancellationToken)
    {
        await deactivateCustomer.HandleAsync(new DeactivateCustomerCommand(id), cancellationToken);
        logger.LogInformation("Customer deactivated. CustomerId={CustomerId}", id);
        return NoContent();
    }

    [HttpGet("customers/{id:guid}/tickets")]
    [Authorize(Policy = Permissions.CustomersHistoryView)]
    public async Task<IActionResult> GetCustomerTickets(Guid id, CancellationToken cancellationToken) =>
        Ok(new ApiResponse<IReadOnlyList<CustomerTicketSummaryDto>>(await getCustomerTickets.HandleAsync(new GetCustomerTicketsQuery(id), cancellationToken)));

    public sealed record CreateCustomerRequest(Guid AccountId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? ExternalReference);
    public sealed record UpdateCustomerRequest(Guid AccountId, string? FirstName, string? LastName, string? Email, string? PhoneNumber, string? ExternalReference);
}
