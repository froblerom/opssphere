using Microsoft.AspNetCore.Mvc;
using OpsSphere.Application.Common.Exceptions;

namespace OpsSphere.IntegrationTests.TestInfrastructure;

[ApiController]
[Route("api/test-diagnostics")]
public sealed class TestDiagnosticsController : ControllerBase
{
    [HttpGet("throw-validation")]
    public IActionResult ThrowValidation() =>
        throw new ValidationException("email", "Email is required.");

    [HttpGet("throw-business-rule")]
    public IActionResult ThrowBusinessRule() =>
        throw new BusinessRuleException("Ticket must be resolved before it can be closed.");

    [HttpGet("throw-not-found")]
    public IActionResult ThrowNotFound() =>
        throw new NotFoundException("Ticket", Guid.NewGuid());

    [HttpGet("throw-conflict")]
    public IActionResult ThrowConflict() =>
        throw new ConflictException("A user with this email already exists.");

    [HttpGet("throw-unhandled")]
    public IActionResult ThrowUnhandled() =>
        throw new InvalidOperationException("Simulated unhandled exception for testing.");
}
