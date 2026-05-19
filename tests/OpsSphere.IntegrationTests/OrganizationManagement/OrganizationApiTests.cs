using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.OrganizationManagement;

public sealed class OrganizationApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string ManagerEmail = "manager.latam@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Admin_CanCreateUpdateAndDeactivateOrganizationHierarchy()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var region = await CreateRegionAsync(client, "andr", "Andromeda Region");
        Assert.Equal("ANDR", region.Code);
        await factory.AssertAuditAsync("RegionCreated", "Region", region.Id);

        var updatedRegionResponse = await client.PutAsJsonAsync($"/api/regions/{region.Id}", new { code = "ANDR-OPS", name = "Andromeda Operations" });
        var updatedRegion = await OpsSphereSqliteFactory.ReadDataAsync<RegionResponse>(updatedRegionResponse);
        Assert.Equal(HttpStatusCode.OK, updatedRegionResponse.StatusCode);
        Assert.Equal("ANDR-OPS", updatedRegion.Code);
        await factory.AssertAuditAsync("RegionUpdated", "Region", region.Id);

        var country = await CreateCountryAsync(client, updatedRegion.Id, "luna", "Lunara");
        await factory.AssertAuditAsync("CountryCreated", "Country", country.Id);

        var updatedCountryResponse = await client.PutAsJsonAsync($"/api/countries/{country.Id}", new { regionId = updatedRegion.Id, code = "LUN", name = "Lunara Prime" });
        var updatedCountry = await OpsSphereSqliteFactory.ReadDataAsync<CountryResponse>(updatedCountryResponse);
        Assert.Equal(HttpStatusCode.OK, updatedCountryResponse.StatusCode);
        Assert.Equal("LUN", updatedCountry.Code);
        await factory.AssertAuditAsync("CountryUpdated", "Country", country.Id);

        var account = await CreateAccountAsync(client, updatedCountry.Id, "orion-care", "Orion Care", "Fictional operations account.");
        await factory.AssertAuditAsync("AccountCreated", "Account", account.Id);

        var updatedAccountResponse = await client.PutAsJsonAsync($"/api/accounts/{account.Id}", new { countryId = updatedCountry.Id, code = "ORION", name = "Orion Support", description = "Updated fictional account." });
        var updatedAccount = await OpsSphereSqliteFactory.ReadDataAsync<AccountResponse>(updatedAccountResponse);
        Assert.Equal(HttpStatusCode.OK, updatedAccountResponse.StatusCode);
        Assert.Equal("ORION", updatedAccount.Code);
        await factory.AssertAuditAsync("AccountUpdated", "Account", account.Id);

        var campaign = await CreateCampaignAsync(client, updatedAccount.Id, updatedCountry.Id, "orion-chat", "Orion Chat", "Fictional campaign.");
        await factory.AssertAuditAsync("CampaignCreated", "Campaign", campaign.Id);

        var updatedCampaignResponse = await client.PutAsJsonAsync($"/api/campaigns/{campaign.Id}", new { accountId = updatedAccount.Id, countryId = updatedCountry.Id, code = "ORION-VOICE", name = "Orion Voice", description = "Updated fictional campaign." });
        var updatedCampaign = await OpsSphereSqliteFactory.ReadDataAsync<CampaignResponse>(updatedCampaignResponse);
        Assert.Equal(HttpStatusCode.OK, updatedCampaignResponse.StatusCode);
        Assert.Equal("ORION-VOICE", updatedCampaign.Code);
        await factory.AssertAuditAsync("CampaignUpdated", "Campaign", campaign.Id);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/api/campaigns/{campaign.Id}/deactivate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/api/accounts/{updatedAccount.Id}/deactivate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/api/countries/{updatedCountry.Id}/deactivate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/api/regions/{updatedRegion.Id}/deactivate", null)).StatusCode);

        await factory.AssertAuditAsync("CampaignDeactivated", "Campaign", campaign.Id);
        await factory.AssertAuditAsync("AccountDeactivated", "Account", updatedAccount.Id);
        await factory.AssertAuditAsync("CountryDeactivated", "Country", updatedCountry.Id);
        await factory.AssertAuditAsync("RegionDeactivated", "Region", updatedRegion.Id);
        await AssertResponseDoesNotExposeSensitiveDataAsync(updatedCampaignResponse);
    }

    [Fact]
    public async Task NonAdmin_CannotCreateUpdateOrDeactivateOrganizationRecords()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var createRegion = await client.PostAsJsonAsync("/api/regions", new { code = "DENY-R", name = "Denied Region" });
        var updateRegion = await client.PutAsJsonAsync($"/api/regions/{SeedIds.Regions.Latam}", new { code = "LATAM", name = "Denied Region Update" });
        var deactivateRegion = await client.PostAsync($"/api/regions/{SeedIds.Regions.Latam}/deactivate", null);
        var createCountry = await client.PostAsJsonAsync("/api/countries", new { regionId = SeedIds.Regions.Latam, code = "DN", name = "Denied Country" });
        var updateCountry = await client.PutAsJsonAsync($"/api/countries/{SeedIds.Countries.Mexico}", new { regionId = SeedIds.Regions.Latam, code = "MX", name = "Denied Country Update" });
        var deactivateCountry = await client.PostAsync($"/api/countries/{SeedIds.Countries.Mexico}/deactivate", null);
        var createAccount = await client.PostAsJsonAsync("/api/accounts", new { countryId = SeedIds.Countries.Mexico, code = "DENY-A", name = "Denied Account" });
        var updateAccount = await client.PutAsJsonAsync($"/api/accounts/{SeedIds.Accounts.NovaBank}", new { countryId = SeedIds.Countries.Mexico, code = "NOVABANK", name = "Denied Account Update" });
        var deactivateAccount = await client.PostAsync($"/api/accounts/{SeedIds.Accounts.NovaBank}/deactivate", null);
        var createCampaign = await client.PostAsJsonAsync("/api/campaigns", new { accountId = SeedIds.Accounts.NovaBank, countryId = SeedIds.Countries.Mexico, code = "DENY-C", name = "Denied Campaign" });
        var updateCampaign = await client.PutAsJsonAsync($"/api/campaigns/{SeedIds.Campaigns.NovaBankCreditCard}", new { accountId = SeedIds.Accounts.NovaBank, countryId = SeedIds.Countries.Mexico, code = "NOVABANK-CC", name = "Denied Campaign Update" });
        var deactivateCampaign = await client.PostAsync($"/api/campaigns/{SeedIds.Campaigns.NovaBankCreditCard}/deactivate", null);

        foreach (var response in new[] { createRegion, updateRegion, deactivateRegion, createCountry, updateCountry, deactivateCountry, createAccount, updateAccount, deactivateAccount, createCampaign, updateCampaign, deactivateCampaign })
        {
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task OrganizationValidation_RejectsRequiredFieldAndDuplicateCodeFailures()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var missing = await client.PostAsJsonAsync("/api/regions", new { code = "", name = "" });
        var missingError = await OpsSphereSqliteFactory.ReadErrorAsync(missing);
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal("validation_error", missingError.Error.Code);
        Assert.Contains(missingError.Error.Details, detail => detail.Field == "code");
        Assert.Contains(missingError.Error.Details, detail => detail.Field == "name");

        await AssertConflictAsync(await client.PostAsJsonAsync("/api/regions", new { code = "LATAM", name = "Duplicate Latam" }));
        await AssertConflictAsync(await client.PostAsJsonAsync("/api/countries", new { regionId = SeedIds.Regions.Latam, code = "MX", name = "Duplicate Mexico" }));
        await AssertConflictAsync(await client.PostAsJsonAsync("/api/accounts", new { countryId = SeedIds.Countries.Mexico, code = "NOVABANK", name = "Duplicate NovaBank" }));
        await AssertConflictAsync(await client.PostAsJsonAsync("/api/campaigns", new { accountId = SeedIds.Accounts.NovaBank, countryId = SeedIds.Countries.Mexico, code = "NOVABANK-CC", name = "Duplicate Campaign" }));
    }

    [Fact]
    public async Task OrganizationParentsAndDeactivationRules_AreEnforced()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);
        var inactiveRegionId = await factory.AddInactiveRegionAsync("VOID-R", "Void Region");
        var inactiveCountryId = await factory.AddInactiveCountryAsync(inactiveRegionId, "VOID-C", "Void Country");
        var inactiveAccountId = await factory.AddInactiveAccountAsync(inactiveCountryId, "VOID-A", "Void Account");

        await AssertValidationAsync(await client.PostAsJsonAsync("/api/countries", new { regionId = Guid.NewGuid(), code = "MISSING-R", name = "Missing Region Country" }), "regionId");
        await AssertValidationAsync(await client.PostAsJsonAsync("/api/countries", new { regionId = inactiveRegionId, code = "INACTIVE-R", name = "Inactive Region Country" }), "regionId");
        await AssertValidationAsync(await client.PostAsJsonAsync("/api/accounts", new { countryId = Guid.NewGuid(), code = "MISSING-C", name = "Missing Country Account" }), "countryId");
        await AssertValidationAsync(await client.PostAsJsonAsync("/api/accounts", new { countryId = inactiveCountryId, code = "INACTIVE-C", name = "Inactive Country Account" }), "countryId");
        await AssertValidationAsync(await client.PostAsJsonAsync("/api/campaigns", new { accountId = Guid.NewGuid(), countryId = SeedIds.Countries.Mexico, code = "MISSING-A", name = "Missing Account Campaign" }), "accountId");
        await AssertValidationAsync(await client.PostAsJsonAsync("/api/campaigns", new { accountId = inactiveAccountId, countryId = inactiveCountryId, code = "INACTIVE-A", name = "Inactive Account Campaign" }), "accountId");
        await AssertValidationAsync(await client.PostAsJsonAsync("/api/campaigns", new { accountId = SeedIds.Accounts.NovaBank, countryId = SeedIds.Countries.UnitedStates, code = "MISMATCH-COUNTRY", name = "Mismatched Country Campaign" }), "countryId");

        await AssertBusinessRuleAsync(await client.PostAsync($"/api/regions/{SeedIds.Regions.Latam}/deactivate", null));
        await AssertBusinessRuleAsync(await client.PostAsync($"/api/countries/{SeedIds.Countries.Mexico}/deactivate", null));
        await AssertBusinessRuleAsync(await client.PostAsync($"/api/accounts/{SeedIds.Accounts.NovaBank}/deactivate", null));
    }

    [Theory]
    [InlineData("manager.latam@opssphere.local", "Region")]
    [InlineData("supervisor.novabank@opssphere.local", "Account")]
    [InlineData("supervisor.novabank@opssphere.local", "Campaign")]
    [InlineData("agent.novabank@opssphere.local", "Account")]
    [InlineData("agent.novabank@opssphere.local", "Campaign")]
    [InlineData("viewer.latam@opssphere.local", "Region")]
    [InlineData("viewer.latam@opssphere.local", "Country")]
    [InlineData("viewer.latam@opssphere.local", "Account")]
    [InlineData("viewer.latam@opssphere.local", "Campaign")]
    public async Task UserScopes_CanBeAssignedAccordingToRole(string email, string scopeType)
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);
        var userId = await factory.GetUserIdAsync(email);

        var response = await client.PutAsJsonAsync($"/api/users/{userId}/scopes", new
        {
            scopes = new[] { CreateScopeRequest(scopeType) }
        });
        var assignment = await OpsSphereSqliteFactory.ReadDataAsync<UserScopeAssignmentResponse>(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(assignment.Scopes, scope => scope.ScopeType == scopeType && scope.IsActive);
        await factory.AssertAuditAsync("UserScopeAssigned", "User", userId);
    }

    [Fact]
    public async Task UserScopes_RejectInvalidRoleInvalidScopeInactiveUserAndInactiveTargets()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);
        var inactiveRegionId = await factory.AddInactiveRegionAsync("SLEEP-R", "Sleeping Region");
        await factory.DeactivateUserAsync(SeedIds.Users.ViewerLatam);

        await AssertValidationAsync(await client.PutAsJsonAsync($"/api/users/{SeedIds.Users.ManagerLatam}/scopes", new { scopes = new[] { CreateScopeRequest("Account") } }), "scopes");
        await AssertValidationAsync(await client.PutAsJsonAsync($"/api/users/{SeedIds.Users.SupervisorNovabank}/scopes", new { scopes = new[] { new { scopeType = "Planet", regionId = SeedIds.Regions.Latam, countryId = (Guid?)null, accountId = (Guid?)null, campaignId = (Guid?)null } } }), "scopes");
        await AssertValidationAsync(await client.PutAsJsonAsync($"/api/users/{SeedIds.Users.ViewerLatam}/scopes", new { scopes = new[] { CreateScopeRequest("Region") } }), "userId");
        await AssertValidationAsync(await client.PutAsJsonAsync($"/api/users/{SeedIds.Users.ManagerLatam}/scopes", new { scopes = new[] { new { scopeType = "Region", regionId = (Guid?)inactiveRegionId, countryId = (Guid?)null, accountId = (Guid?)null, campaignId = (Guid?)null } } }), "scopes");
    }

    [Fact]
    public async Task UserScopes_WriteAuditForAssignDeactivateAndReactivate()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var assignAccount = await client.PutAsJsonAsync($"/api/users/{SeedIds.Users.AgentNovabank}/scopes", new { scopes = new[] { CreateScopeRequest("Account") } });
        Assert.Equal(HttpStatusCode.OK, assignAccount.StatusCode);
        await factory.AssertAuditAsync("UserScopeAssigned", "User", SeedIds.Users.AgentNovabank);
        await factory.AssertAuditAsync("UserScopeDeactivated", "User", SeedIds.Users.AgentNovabank);

        var reactivateCampaign = await client.PutAsJsonAsync($"/api/users/{SeedIds.Users.AgentNovabank}/scopes", new
        {
            scopes = new[]
            {
                new { scopeType = "Campaign", regionId = (Guid?)null, countryId = (Guid?)null, accountId = (Guid?)null, campaignId = (Guid?)SeedIds.Campaigns.NovaBankCreditCard }
            }
        });
        Assert.Equal(HttpStatusCode.OK, reactivateCampaign.StatusCode);
        await factory.AssertAuditAsync("UserScopeUpdated", "User", SeedIds.Users.AgentNovabank);
    }

    [Fact]
    public async Task ScopedReads_FilterOrganizationRecordsServerSide()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var manager = await factory.CreateAuthenticatedClientAsync(ManagerEmail);
        var viewer = await factory.CreateAuthenticatedClientAsync(ViewerEmail);
        var supervisor = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);
        var agent = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var managerRegions = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<RegionResponse>>(await manager.GetAsync("/api/regions"));
        var viewerCountries = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<CountryResponse>>(await viewer.GetAsync("/api/countries"));
        var supervisorAccounts = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<AccountResponse>>(await supervisor.GetAsync("/api/accounts"));
        var supervisorCampaigns = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<CampaignResponse>>(await supervisor.GetAsync("/api/campaigns"));
        var agentRegions = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<RegionResponse>>(await agent.GetAsync("/api/regions"));
        var agentCountries = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<CountryResponse>>(await agent.GetAsync("/api/countries"));
        var agentAccounts = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<AccountResponse>>(await agent.GetAsync("/api/accounts"));
        var agentCampaigns = await OpsSphereSqliteFactory.ReadDataAsync<IReadOnlyList<CampaignResponse>>(await agent.GetAsync("/api/campaigns"));

        Assert.Single(managerRegions);
        Assert.Contains(managerRegions, region => region.Code == "LATAM");
        Assert.DoesNotContain(viewerCountries, country => country.Code == "US");
        Assert.Single(supervisorAccounts);
        Assert.Contains(supervisorAccounts, account => account.Code == "NOVABANK");
        Assert.Equal(2, supervisorCampaigns.Count(campaign => campaign.AccountCode == "NOVABANK"));
        Assert.Single(agentRegions);
        Assert.Contains(agentRegions, region => region.Code == "LATAM");
        Assert.Single(agentCountries);
        Assert.Contains(agentCountries, country => country.Code == "MX");
        Assert.Single(agentAccounts);
        Assert.Contains(agentAccounts, account => account.Code == "NOVABANK");
        Assert.Single(agentCampaigns);
        Assert.Contains(agentCampaigns, campaign => campaign.Code == "NOVABANK-CC");
    }

    [Fact]
    public async Task OrganizationResponses_UseEnvelopeAndDoNotExposeSecrets()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var regionResponse = await client.GetAsync("/api/regions");
        var json = await regionResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, regionResponse.StatusCode);
        Assert.Contains("\"data\"", json, StringComparison.OrdinalIgnoreCase);
        await AssertResponseDoesNotExposeSensitiveDataAsync(regionResponse);
    }

    private static object CreateScopeRequest(string scopeType) => scopeType switch
    {
        "Region" => new { scopeType, regionId = (Guid?)SeedIds.Regions.NorthAmerica, countryId = (Guid?)null, accountId = (Guid?)null, campaignId = (Guid?)null },
        "Country" => new { scopeType, regionId = (Guid?)null, countryId = (Guid?)SeedIds.Countries.UnitedStates, accountId = (Guid?)null, campaignId = (Guid?)null },
        "Account" => new { scopeType, regionId = (Guid?)null, countryId = (Guid?)null, accountId = (Guid?)SeedIds.Accounts.Streamly, campaignId = (Guid?)null },
        "Campaign" => new { scopeType, regionId = (Guid?)null, countryId = (Guid?)null, accountId = (Guid?)null, campaignId = (Guid?)SeedIds.Campaigns.StreamlyCreator },
        _ => throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null)
    };

    private static async Task<RegionResponse> CreateRegionAsync(HttpClient client, string code, string name)
    {
        var response = await client.PostAsJsonAsync("/api/regions", new { code, name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await OpsSphereSqliteFactory.ReadDataAsync<RegionResponse>(response);
    }

    private static async Task<CountryResponse> CreateCountryAsync(HttpClient client, Guid regionId, string code, string name)
    {
        var response = await client.PostAsJsonAsync("/api/countries", new { regionId, code, name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await OpsSphereSqliteFactory.ReadDataAsync<CountryResponse>(response);
    }

    private static async Task<AccountResponse> CreateAccountAsync(HttpClient client, Guid countryId, string code, string name, string description)
    {
        var response = await client.PostAsJsonAsync("/api/accounts", new { countryId, code, name, description });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await OpsSphereSqliteFactory.ReadDataAsync<AccountResponse>(response);
    }

    private static async Task<CampaignResponse> CreateCampaignAsync(HttpClient client, Guid accountId, Guid countryId, string code, string name, string description)
    {
        var response = await client.PostAsJsonAsync("/api/campaigns", new { accountId, countryId, code, name, description });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await OpsSphereSqliteFactory.ReadDataAsync<CampaignResponse>(response);
    }

    private static async Task AssertConflictAsync(HttpResponseMessage response)
    {
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("conflict", error.Error.Code);
    }

    private static async Task AssertValidationAsync(HttpResponseMessage response, string field)
    {
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details, detail => detail.Field == field);
    }

    private static async Task AssertBusinessRuleAsync(HttpResponseMessage response)
    {
        var error = await OpsSphereSqliteFactory.ReadErrorAsync(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("business_rule_violation", error.Error.Code);
    }

private static async Task AssertResponseDoesNotExposeSensitiveDataAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        foreach (var term in new[] { "passwordHash", "temporaryPassword", "accessToken", "refreshToken", "jwt", "secret", OpsSphereSqliteFactory.JwtSigningKey })
        {
            Assert.DoesNotContain(term, json, StringComparison.OrdinalIgnoreCase);
        }
    }
    private sealed record RegionResponse(Guid Id, string Code, string Name, bool IsActive);
    private sealed record CountryResponse(Guid Id, Guid RegionId, string RegionCode, string Code, string Name, bool IsActive);
    private sealed record AccountResponse(Guid Id, Guid CountryId, string CountryCode, Guid RegionId, string RegionCode, string Code, string Name, string? Description, bool IsActive);
    private sealed record CampaignResponse(Guid Id, Guid AccountId, string AccountCode, Guid CountryId, string CountryCode, Guid RegionId, string RegionCode, string Code, string Name, string? Description, bool IsActive);
    private sealed record UserScopeAssignmentResponse(Guid UserId, string Email, bool IsActive, IReadOnlyList<string> Roles, IReadOnlyList<UserScopeResponse> Scopes);
    private sealed record UserScopeResponse(Guid Id, string ScopeType, Guid? RegionId, Guid? CountryId, Guid? AccountId, Guid? CampaignId, bool IsActive);
}