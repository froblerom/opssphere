using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.CustomerManagement;

public sealed class CustomerManagementApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Admin_CanCreateUpdateAndDeactivateCustomer()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var customer = await CreateCustomerAsync(client, SeedIds.Accounts.NovaBank, "Lucia", "Reyes", "l.reyes@fictional.test", null, "NB-NEW-001");
        Assert.Equal("Lucia", customer.FirstName);
        Assert.Equal("Reyes", customer.LastName);
        Assert.Equal("NB-NEW-001", customer.ExternalReference);
        Assert.True(customer.IsActive);
        await factory.AssertAuditAsync("CustomerCreated", "Customer", customer.Id);

        var updateResponse = await client.PutAsJsonAsync($"/api/customers/{customer.Id}", new
        {
            accountId = SeedIds.Accounts.NovaBank,
            firstName = "Lucia",
            lastName = "Reyes-Updated",
            email = "l.reyes.updated@fictional.test",
            phoneNumber = (string?)null,
            externalReference = "NB-NEW-001-U"
        });
        var updated = await OpsSphereSqliteFactory.ReadDataAsync<CustomerResponse>(updateResponse);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal("Reyes-Updated", updated.LastName);
        Assert.Equal("NB-NEW-001-U", updated.ExternalReference);
        await factory.AssertAuditAsync("CustomerUpdated", "Customer", customer.Id);

        var deactivateResponse = await client.PostAsync($"/api/customers/{customer.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);
        await factory.AssertAuditAsync("CustomerDeactivated", "Customer", customer.Id);

        await AssertResponseDoesNotExposeSensitiveDataAsync(updateResponse);
    }

    [Fact]
    public async Task Viewer_CanReadCustomersButNotWriteThem()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var viewer = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        var listResponse = await viewer.GetAsync("/api/customers");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var createResponse = await viewer.PostAsJsonAsync("/api/customers", new
        {
            accountId = SeedIds.Accounts.NovaBank,
            firstName = "Denied",
            lastName = "Write",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);

        var updateResponse = await viewer.PutAsJsonAsync($"/api/customers/{SeedIds.Customers.NovaBankCustomer1}", new
        {
            accountId = SeedIds.Accounts.NovaBank,
            firstName = "Denied",
            lastName = "Update",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);

        var deactivateResponse = await viewer.PostAsync($"/api/customers/{SeedIds.Customers.NovaBankCustomer1}/deactivate", null);
        Assert.Equal(HttpStatusCode.Forbidden, deactivateResponse.StatusCode);
    }

    [Fact]
    public async Task CustomerValidation_RejectsRequiredFieldFailures()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var missing = await client.PostAsJsonAsync("/api/customers", new
        {
            accountId = SeedIds.Accounts.NovaBank,
            firstName = "",
            lastName = "",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        var missingError = await OpsSphereSqliteFactory.ReadErrorAsync(missing);
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal("validation_error", missingError.Error.Code);
        Assert.Contains(missingError.Error.Details, d => d.Field == "firstName");
        Assert.Contains(missingError.Error.Details, d => d.Field == "lastName");
    }

    [Fact]
    public async Task CustomerCreate_RejectsInactiveOrMissingAccount()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var inactiveAccountId = await factory.AddInactiveAccountAsync("VOID-CA", "Void Customer Account");

        var missingAccount = await client.PostAsJsonAsync("/api/customers", new
        {
            accountId = Guid.NewGuid(),
            firstName = "Test",
            lastName = "Customer",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        await AssertValidationAsync(missingAccount, "accountId");

        var inactiveAccount = await client.PostAsJsonAsync("/api/customers", new
        {
            accountId = inactiveAccountId,
            firstName = "Test",
            lastName = "Customer",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        await AssertValidationAsync(inactiveAccount, "accountId");
    }

    [Fact]
    public async Task ScopedReads_FilterCustomersByAccountScope()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var agentCustomers = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<CustomerResponse>>(await agent.GetAsync("/api/customers"));
        var supervisorCustomers = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<CustomerResponse>>(await supervisor.GetAsync("/api/customers"));

        Assert.All(agentCustomers, c => Assert.Equal(SeedIds.Accounts.NovaBank, c.AccountId));
        Assert.All(supervisorCustomers, c => Assert.Equal(SeedIds.Accounts.NovaBank, c.AccountId));
        Assert.DoesNotContain(agentCustomers, c => c.AccountId == SeedIds.Accounts.Streamly);
    }

    [Fact]
    public async Task CrossScopeCustomer_ReturnsNotFoundToNonAdmin()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var getResponse = await agent.GetAsync($"/api/customers/{SeedIds.Customers.StreamlyCustomer1}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var updateResponse = await agent.PutAsJsonAsync($"/api/customers/{SeedIds.Customers.StreamlyCustomer1}", new
        {
            accountId = SeedIds.Accounts.Streamly,
            firstName = "Cross",
            lastName = "Scope",
            email = (string?)null,
            phoneNumber = (string?)null,
            externalReference = (string?)null
        });
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deactivateResponse = await agent.PostAsync($"/api/customers/{SeedIds.Customers.StreamlyCustomer1}/deactivate", null);
        Assert.Equal(HttpStatusCode.NotFound, deactivateResponse.StatusCode);
    }

    [Fact]
    public async Task CustomerTickets_ReturnsSeededDemoHistory()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await client.GetAsync($"/api/customers/{SeedIds.Customers.NovaBankCustomer1}/tickets");
        var tickets = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<CustomerTicketSummaryResponse>>(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(6, tickets.Count);
        Assert.Contains(tickets, ticket => ticket.TicketNumber == "OPS-000001" && ticket.Status == "Open");
        Assert.Contains(tickets, ticket => ticket.TicketNumber == "OPS-000006" && ticket.Status == "Closed");
    }

    [Fact]
    public async Task CustomerAuditLogs_DoNotContainPii()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var customer = await CreateCustomerAsync(client, SeedIds.Accounts.NovaBank, "AuditTest", "PiiCheck", "pii.test@fictional.test", "+1-555-000-0001", "AUDIT-001");
        await factory.AssertAuditDoesNotContainPiiAsync(customer.Id);
    }

    [Fact]
    public async Task CustomerResponses_UseEnvelopeAndDoNotExposeSecrets()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await client.GetAsync("/api/customers");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"data\"", json, StringComparison.OrdinalIgnoreCase);
        await AssertResponseDoesNotExposeSensitiveDataAsync(response);
    }

    private static async Task<CustomerResponse> CreateCustomerAsync(HttpClient client, Guid accountId, string firstName, string lastName, string? email, string? phoneNumber, string? externalReference)
    {
        var response = await client.PostAsJsonAsync("/api/customers", new { accountId, firstName, lastName, email, phoneNumber, externalReference });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await OpsSphereSqliteFactory.ReadDataAsync<CustomerResponse>(response);
    }

    private static async Task AssertValidationAsync(HttpResponseMessage response, string field)
    {
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, detail => detail.Field == field);
    }

private static async Task AssertResponseDoesNotExposeSensitiveDataAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        foreach (var term in new[] { "passwordHash", "temporaryPassword", "accessToken", "refreshToken", "jwt", "secret", OpsSphereSqliteFactory.JwtSigningKey })
        {
            Assert.DoesNotContain(term, json, StringComparison.OrdinalIgnoreCase);
        }
    }
    private sealed record CustomerResponse(Guid Id, Guid AccountId, string AccountCode, string AccountName, string FirstName, string LastName, string? Email, string? PhoneNumber, string? ExternalReference, bool IsActive);
    private sealed record CustomerTicketSummaryResponse(Guid Id, string TicketNumber, string Status, string Priority, string SlaState, DateTime CreatedAt, DateTime? ResolvedAt, DateTime? ClosedAt);
}