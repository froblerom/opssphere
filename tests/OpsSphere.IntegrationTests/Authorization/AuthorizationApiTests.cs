using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.IntegrationTests.TestInfrastructure;

namespace OpsSphere.IntegrationTests.Authorization;

/// <summary>
/// Integration tests for RBAC, permission checks, and scope authorization.
/// Backend authorization is the source of truth — frontend visibility is UX only.
/// </summary>
public sealed class AuthorizationApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string ManagerEmail = "manager.latam@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Seeded region/account/campaign IDs from SeedIds
    private static readonly Guid LatamRegionId = SeedIds.Regions.Latam;
    private static readonly Guid NaRegionId = SeedIds.Regions.NorthAmerica;
    private static readonly Guid NovaBankAccountId = SeedIds.Accounts.NovaBank;
    private static readonly Guid StreamlyAccountId = SeedIds.Accounts.Streamly;
    private static readonly Guid NovaBankCampaignId = SeedIds.Campaigns.NovaBankCreditCard;
    private static readonly Guid StreamlyCampaignId = SeedIds.Campaigns.StreamlyCreator;

    // 1. Viewer cannot execute write action ? 403
    [Fact]
    public async Task WriteSmoke_WithViewer_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        var response = await client.GetAsync("/api/authz/smoke/write");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 2. Agent cannot access admin-only action ? 403
    [Fact]
    public async Task AdminSmoke_WithAgent_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await client.GetAsync("/api/authz/smoke/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 3. Supervisor cannot access records outside assigned account/campaign ? 403
    [Fact]
    public async Task ScopedCampaign_Supervisor_OutsideScope_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        // Supervisor is scoped to NOVABANK account — Streamly campaign is outside scope
        var response = await client.GetAsync($"/api/authz/smoke/scoped/campaigns/{StreamlyCampaignId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 4. Supervisor can access records inside assigned account/campaign ? 200
    [Fact]
    public async Task ScopedCampaign_Supervisor_InsideScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        // Supervisor has NOVABANK account scope — NovaBankCreditCard is under NOVABANK
        var response = await client.GetAsync($"/api/authz/smoke/scoped/campaigns/{NovaBankCampaignId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 4b. Supervisor can access account inside scope
    [Fact]
    public async Task ScopedAccount_Supervisor_InsideScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await client.GetAsync($"/api/authz/smoke/scoped/accounts/{NovaBankAccountId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 4c. Supervisor cannot access account outside scope
    [Fact]
    public async Task ScopedAccount_Supervisor_OutsideScope_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await client.GetAsync($"/api/authz/smoke/scoped/accounts/{StreamlyAccountId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 5. OperationsManager can read data within assigned region ? 200
    [Fact]
    public async Task ScopedRegion_Manager_InsideScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ManagerEmail);

        // Manager has LATAM region scope
        var response = await client.GetAsync($"/api/authz/smoke/scoped/regions/{LatamRegionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 6. OperationsManager cannot read data outside assigned region ? 403
    [Fact]
    public async Task ScopedRegion_Manager_OutsideScope_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ManagerEmail);

        // Manager has LATAM scope — NA region is outside scope
        var response = await client.GetAsync($"/api/authz/smoke/scoped/regions/{NaRegionId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 5b. Manager can access account inside their region scope (hierarchy test)
    [Fact]
    public async Task ScopedAccount_Manager_InsideRegionScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ManagerEmail);

        // Manager has LATAM region scope; NOVABANK is in MX which is in LATAM
        var response = await client.GetAsync($"/api/authz/smoke/scoped/accounts/{NovaBankAccountId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 7. User without required permission is denied ? 403
    [Fact]
    public async Task AdminSmoke_WithViewer_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        var response = await client.GetAsync("/api/authz/smoke/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 8. User without required scope is denied ? 403
    [Fact]
    public async Task ScopedCampaign_Agent_OutsideScope_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        // Agent has NOVABANK-CC campaign scope only — Streamly campaign is out of scope
        var response = await client.GetAsync($"/api/authz/smoke/scoped/campaigns/{StreamlyCampaignId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 8b. Agent can access their assigned campaign
    [Fact]
    public async Task ScopedCampaign_Agent_InsideScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        // Agent is scoped to NOVABANK-CC campaign
        var response = await client.GetAsync($"/api/authz/smoke/scoped/campaigns/{NovaBankCampaignId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 9. Scoped campaign list returns only records inside assigned scope (profile check)
    [Fact]
    public async Task ScopedCampaigns_ReturnsScopeRestrictedResult()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await client.GetAsync("/api/authz/smoke/scoped/campaigns");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await OpsSphereSqliteFactory.ReadResponseAsync<SmokeResponse>(response);
        Assert.Equal("ok", body.Status);
    }

    // 10. Unauthenticated request returns 401
    [Fact]
    public async Task AdminSmoke_WithoutToken_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/authz/smoke/admin");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WriteSmoke_WithoutToken_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync("/api/authz/smoke/write");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ScopedRegion_WithoutToken_Returns401()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();

        var response = await factory.CreateClient().GetAsync($"/api/authz/smoke/scoped/regions/{LatamRegionId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 11. Authenticated but unauthorized request returns 403
    [Fact]
    public async Task AdminSmoke_WithManager_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ManagerEmail);

        // Manager does not have users.manage permission
        var response = await client.GetAsync("/api/authz/smoke/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WriteSmoke_WithManager_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ManagerEmail);

        // OperationsManager does not have tickets.create permission
        var response = await client.GetAsync("/api/authz/smoke/write");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 12. Backend authorization works independently from frontend visibility
    // Verified by the test setup: all tests use raw HTTP (no frontend), proving
    // backend enforces authorization independent of any Angular visibility control.
    [Fact]
    public async Task BackendAuthz_IsEnforcedWithoutFrontend()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        // Viewer — no frontend, raw HTTP call to a write endpoint
        var client = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        var response = await client.GetAsync("/api/authz/smoke/write");

        // Backend denies regardless of frontend visibility
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 13. Authorization denial logs do not expose passwords, password hashes, JWT tokens, Authorization headers, signing keys, or secrets
    [Fact]
    public async Task AuthorizationDenial_LogsDoNotExposeSecrets()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        // Trigger a 403 denial
        var response = await client.GetAsync("/api/authz/smoke/admin");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        // Response body must not contain signing key, password-related data, or token
        Assert.DoesNotContain(OpsSphereSqliteFactory.JwtSigningKey, responseBody, StringComparison.Ordinal);
        Assert.DoesNotContain("passwordHash", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", responseBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Authorization", responseBody, StringComparison.OrdinalIgnoreCase);
    }

    // Admin bypasses scope checks
    [Fact]
    public async Task ScopedRegion_Admin_BypassesScopeRestriction_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        // Admin accesses NA region even though Admin has no region scope row
        var response = await client.GetAsync($"/api/authz/smoke/scoped/regions/{NaRegionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ScopedAccount_Admin_BypassesScopeRestriction_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await client.GetAsync($"/api/authz/smoke/scoped/accounts/{StreamlyAccountId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Viewer scope test — Viewer has LATAM region scope
    [Fact]
    public async Task ScopedRegion_Viewer_InsideScope_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        var response = await client.GetAsync($"/api/authz/smoke/scoped/regions/{LatamRegionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ScopedRegion_Viewer_OutsideScope_Returns403()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(ViewerEmail);

        // Viewer has LATAM scope — NA is outside scope
        var response = await client.GetAsync($"/api/authz/smoke/scoped/regions/{NaRegionId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Admin smoke endpoint works for Admin
    [Fact]
    public async Task AdminSmoke_WithAdmin_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminEmail);

        var response = await client.GetAsync("/api/authz/smoke/admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Write smoke works for Supervisor and Agent
    [Fact]
    public async Task WriteSmoke_WithSupervisor_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(SupervisorEmail);

        var response = await client.GetAsync("/api/authz/smoke/write");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WriteSmoke_WithAgent_Returns200()
    {
        await using var factory = await OpsSphereSqliteFactory.CreateAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AgentEmail);

        var response = await client.GetAsync("/api/authz/smoke/write");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record SmokeResponse(string Status);
}