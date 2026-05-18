using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;

namespace OpsSphere.IntegrationTests.TicketManagement;

public sealed class TicketCommentApiTests
{
    private const string AdminEmail = "admin@opssphere.local";
    private const string AgentEmail = "agent.novabank@opssphere.local";
    private const string SupervisorEmail = "supervisor.novabank@opssphere.local";
    private const string ViewerEmail = "viewer.latam@opssphere.local";
    private const string ManagerEmail = "manager.latam@opssphere.local";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Agent_CanAddComment_WithinScope_Returns200()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Agent comment");

        var response = await AddCommentAsync(agent, ticket.Id, "Agent internal note");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<AddInternalCommentResponse>(response);
        Assert.Equal(ticket.Id, result.TicketId);
        Assert.Equal(ticket.TicketNumber, result.TicketNumber);
        Assert.Equal(SeedIds.Users.AgentNovabank, result.AuthorUserId);
        Assert.Equal("Diego Santos", result.AuthorDisplayName);
        Assert.Equal("Agent internal note", result.Body);
        Assert.Equal("Comment added successfully.", result.Message);
    }

    [Fact]
    public async Task Supervisor_CanAddComment_WithinScope_Returns200()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Supervisor comment");

        var response = await AddCommentAsync(supervisor, ticket.Id, "Supervisor note");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<AddInternalCommentResponse>(response);
        Assert.Equal(SeedIds.Users.SupervisorNovabank, result.AuthorUserId);
        Assert.Equal("Lina Calderon", result.AuthorDisplayName);
    }

    [Fact]
    public async Task OperationsManager_CanAddComment_WithinScope_Returns200()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var manager = await CreateAuthenticatedClientAsync(factory, ManagerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Manager scoped comment");

        var response = await AddCommentAsync(manager, ticket.Id, "Manager note");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<AddInternalCommentResponse>(response);
        Assert.Equal(SeedIds.Users.ManagerLatam, result.AuthorUserId);
        Assert.Equal("Mateo Rios", result.AuthorDisplayName);
    }

    [Fact]
    public async Task Admin_CannotAddComment_Returns403()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var admin = await CreateAuthenticatedClientAsync(factory, AdminEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Admin comment forbidden");

        var response = await AddCommentAsync(admin, ticket.Id, "Admin note");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotAddComment_Returns403()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Viewer comment forbidden");

        var response = await AddCommentAsync(viewer, ticket.Id, "Viewer note");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_CannotAddComment_Returns401()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Unauthenticated comment forbidden");
        var anonymous = factory.CreateClient();

        var response = await AddCommentAsync(anonymous, ticket.Id, "Anonymous note");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddComment_EmptyBody_Returns400_WithValidationError()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Empty comment body");

        var response = await AddCommentAsync(agent, ticket.Id, string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "body");
    }

    [Fact]
    public async Task AddComment_WhitespaceBody_Returns400_WithValidationError()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Whitespace comment body");

        var response = await AddCommentAsync(agent, ticket.Id, "   ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "body");
    }

    [Fact]
    public async Task AddComment_TooLongBody_Returns400_WithValidationError()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Long comment body");

        var response = await AddCommentAsync(agent, ticket.Id, new string('a', 5001));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("validation_error", error.Error.Code);
        Assert.Contains(error.Error.Details ?? [], d => d.Field == "body");
    }

    [Fact]
    public async Task AddComment_CrossScope_Returns404()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.Streamly,
            SeedIds.Campaigns.StreamlyCreator,
            SeedIds.Customers.StreamlyCustomer1,
            TicketStatus.Open,
            "Out-of-scope comment");

        var response = await AddCommentAsync(supervisor, outOfScopeTicketId, "Supervisor out-of-scope note");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddComment_ClosedTicket_Returns400_WithBusinessRuleViolation()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Closed comment rejection");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Closed);

        var response = await AddCommentAsync(agent, ticket.Id, "Closed ticket note");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
        Assert.Contains("closed", error.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddComment_PersistsAuthorAndTimestamp()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Comment persistence");
        var before = DateTime.UtcNow;

        var response = await AddCommentAsync(agent, ticket.Id, "Persisted note");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<AddInternalCommentResponse>(response);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var comment = await db.TicketComments.AsNoTracking().SingleAsync(c => c.Id == result.CommentId);
        Assert.Equal(SeedIds.Users.AgentNovabank, comment.AuthorUserId);
        Assert.True(comment.CreatedAt >= before.AddSeconds(-1));
        Assert.True(comment.CreatedAt <= DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task AddComment_TrimsBodyBeforePersistence()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Trimmed comment");

        var response = await AddCommentAsync(agent, ticket.Id, "  Trimmed body  ");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<AddInternalCommentResponse>(response);
        Assert.Equal("Trimmed body", result.Body);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var comment = await db.TicketComments.AsNoTracking().SingleAsync(c => c.Id == result.CommentId);
        Assert.Equal("Trimmed body", comment.Body);
    }

    [Fact]
    public async Task AddComment_CreatesAuditLog_InternalCommentAdded()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Comment audit action");

        var response = await AddCommentAsync(agent, ticket.Id, "Audited note");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(log => log.Action == "InternalCommentAdded" && log.EntityType == "Ticket" && log.EntityId == ticket.Id);

        Assert.NotNull(audit);
        Assert.Null(audit.PreviousValue);
        Assert.False(string.IsNullOrWhiteSpace(audit.CorrelationId));
    }

    [Fact]
    public async Task AddComment_AuditLogDoesNotContainCommentBodyOrPii()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        const string sensitiveSubject = "CommentSensitiveSubject-ABC123";
        const string sensitiveDescription = "CommentSensitiveDescription-XYZ987";
        const string sensitiveBody = "SensitiveCommentBody-SECRET456";
        var ticket = await CreateNovaBankTicketAsync(agent, sensitiveSubject, sensitiveDescription);

        var response = await AddCommentAsync(agent, ticket.Id, sensitiveBody);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await ReadDataAsync<AddInternalCommentResponse>(response);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var audit = await db.AuditLogs
            .AsNoTracking()
            .SingleAsync(log => log.Action == "InternalCommentAdded" && log.EntityId == ticket.Id);
        var combined = $"{audit.PreviousValue}{audit.NewValue}";

        Assert.DoesNotContain(sensitiveBody, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(sensitiveSubject, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(sensitiveDescription, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Carlos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Mendez", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Diego", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Santos", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AgentEmail, combined, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(audit.NewValue ?? "{}");
        var properties = document.RootElement.EnumerateObject().Select(p => p.Name).Order().ToArray();
        Assert.Equal(["authorUserId", "commentId"], properties);
        Assert.Equal(result.CommentId, document.RootElement.GetProperty("commentId").GetGuid());
        Assert.Equal(SeedIds.Users.AgentNovabank, document.RootElement.GetProperty("authorUserId").GetGuid());
    }

    [Fact]
    public async Task GetComments_WithinScope_ReturnsComments()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Comment list");
        await AddCommentAsync(agent, ticket.Id, "First listed note");

        var response = await agent.GetAsync($"/api/tickets/{ticket.Id}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await ReadDataAsync<IReadOnlyList<TicketCommentDto>>(response);
        var comment = Assert.Single(comments);
        Assert.Equal("First listed note", comment.Body);
        Assert.Equal("Diego Santos", comment.AuthorDisplayName);
    }

    [Fact]
    public async Task GetComments_WhenNoneExist_ReturnsEmptyList()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "No comments");

        var response = await agent.GetAsync($"/api/tickets/{ticket.Id}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await ReadDataAsync<IReadOnlyList<TicketCommentDto>>(response);
        Assert.Empty(comments);
    }

    [Fact]
    public async Task GetComments_ReturnsChronologicalAscending()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Chronological comments");
        await AddCommentDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Third", DateTime.UtcNow.AddMinutes(3));
        await AddCommentDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "First", DateTime.UtcNow.AddMinutes(1));
        await AddCommentDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Second", DateTime.UtcNow.AddMinutes(2));

        var response = await agent.GetAsync($"/api/tickets/{ticket.Id}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await ReadDataAsync<IReadOnlyList<TicketCommentDto>>(response);
        Assert.Equal(["First", "Second", "Third"], comments.Select(c => c.Body).ToArray());
    }

    [Fact]
    public async Task Viewer_CanListComments_WithinScope()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var viewer = await CreateAuthenticatedClientAsync(factory, ViewerEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Viewer comment list");
        await AddCommentAsync(supervisor, ticket.Id, "Viewer visible note");

        var response = await viewer.GetAsync($"/api/tickets/{ticket.Id}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await ReadDataAsync<IReadOnlyList<TicketCommentDto>>(response);
        Assert.Contains(comments, c => c.Body == "Viewer visible note");
    }

    [Fact]
    public async Task GetComments_CrossScope_Returns404()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var outOfScopeTicketId = await AddTicketDirectlyAsync(
            factory,
            SeedIds.Accounts.Streamly,
            SeedIds.Campaigns.StreamlyCreator,
            SeedIds.Customers.StreamlyCustomer1,
            TicketStatus.Open,
            "Out-of-scope comment list");

        var response = await supervisor.GetAsync($"/api/tickets/{outOfScopeTicketId}/comments");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetComments_DoesNotReturnSoftDeletedComments()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Soft deleted comments");
        await AddCommentDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Visible note", DateTime.UtcNow, isDeleted: false);
        await AddCommentDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Deleted note", DateTime.UtcNow.AddMinutes(1), isDeleted: true);

        var response = await agent.GetAsync($"/api/tickets/{ticket.Id}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await ReadDataAsync<IReadOnlyList<TicketCommentDto>>(response);
        Assert.Contains(comments, c => c.Body == "Visible note");
        Assert.DoesNotContain(comments, c => c.Body == "Deleted note");
    }

    [Fact]
    public async Task GetComments_ClosedTicket_StillAllowedWithinScope()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var agent = await CreateAuthenticatedClientAsync(factory, AgentEmail);
        var ticket = await CreateNovaBankTicketAsync(agent, "Closed comment list");
        await AddCommentDirectlyAsync(factory, ticket.Id, SeedIds.Users.AgentNovabank, "Closed ticket visible note", DateTime.UtcNow);
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Closed);

        var response = await agent.GetAsync($"/api/tickets/{ticket.Id}/comments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await ReadDataAsync<IReadOnlyList<TicketCommentDto>>(response);
        Assert.Contains(comments, c => c.Body == "Closed ticket visible note");
    }

    [Fact]
    public async Task AddComment_ClosedTicket_Rejected()
    {
        await using var factory = await TicketCommentApiFactory.CreateAsync();
        var supervisor = await CreateAuthenticatedClientAsync(factory, SupervisorEmail);
        var ticket = await CreateNovaBankTicketAsync(supervisor, "Closed duplicate rejection");
        await SetTicketStatusAsync(factory, ticket.Id, TicketStatus.Closed);

        var response = await AddCommentAsync(supervisor, ticket.Id, "Rejected closed note");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("business_rule_violation", error.Error.Code);
    }

    private static Task<HttpResponseMessage> AddCommentAsync(HttpClient client, Guid ticketId, string? body)
    {
        return client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", new
        {
            body
        });
    }

    private static async Task<CreateTicketResult> CreateNovaBankTicketAsync(
        HttpClient client,
        string subject,
        string? description = null) =>
        await CreateTicketAsync(
            client,
            SeedIds.Customers.NovaBankCustomer1,
            SeedIds.Accounts.NovaBank,
            SeedIds.Campaigns.NovaBankCreditCard,
            "Support",
            "Normal",
            subject,
            description ?? $"Description for {subject}");

    private static async Task<CreateTicketResult> CreateTicketAsync(
        HttpClient client,
        Guid customerId,
        Guid accountId,
        Guid campaignId,
        string category,
        string priority,
        string subject,
        string description)
    {
        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            customerId,
            accountId,
            campaignId,
            category,
            priority,
            subject,
            description
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadDataAsync<CreateTicketResult>(response);
    }

    private static async Task<Guid> AddTicketDirectlyAsync(
        TicketCommentApiFactory factory,
        Guid accountId,
        Guid campaignId,
        Guid customerId,
        TicketStatus status,
        string subject)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var campaign = await db.Campaigns
            .AsNoTracking()
            .Where(c => c.Id == campaignId)
            .Select(c => new { c.CountryId, c.Country.RegionId })
            .SingleAsync();
        var ticketId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        db.Tickets.Add(new Ticket
        {
            Id = ticketId,
            TicketNumber = $"OPS-20990101-{Random.Shared.Next(1, 999999):D6}",
            CustomerId = customerId,
            RegionId = campaign.RegionId,
            CountryId = campaign.CountryId,
            AccountId = accountId,
            CampaignId = campaignId,
            CreatedByUserId = SeedIds.Users.SupervisorNovabank,
            Category = "Support",
            Priority = TicketPriority.Normal,
            Status = status,
            Subject = subject,
            Description = $"Description for {subject}",
            SlaState = SlaState.WithinSla,
            SlaDueAt = now.AddHours(24),
            IsEscalated = false,
            IsDeleted = false,
            CreatedAt = now
        });
        db.TicketStatusHistory.Add(new TicketStatusHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            PreviousStatus = null,
            NewStatus = status.ToString(),
            ChangedByUserId = SeedIds.Users.SupervisorNovabank,
            ChangeReason = "Test arrange",
            CreatedAt = now
        });
        db.TicketSlaStates.Add(new TicketSlaState
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            SlaPolicyId = null,
            StartedAt = now,
            DueAt = now.AddHours(24),
            State = SlaState.WithinSla.ToString()
        });
        await db.SaveChangesAsync();
        return ticketId;
    }

    private static async Task AddCommentDirectlyAsync(
        TicketCommentApiFactory factory,
        Guid ticketId,
        Guid authorUserId,
        string body,
        DateTime createdAt,
        bool isDeleted = false)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorUserId = authorUserId,
            Body = body,
            IsDeleted = isDeleted,
            DeletedAt = isDeleted ? createdAt.AddMinutes(1) : null,
            CreatedAt = createdAt
        };
        db.TicketComments.Add(comment);
        await db.SaveChangesAsync();

        comment.CreatedAt = createdAt;
        await db.SaveChangesAsync();
    }

    private static async Task SetTicketStatusAsync(TicketCommentApiFactory factory, Guid ticketId, TicketStatus status)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
        var ticket = await db.Tickets.SingleAsync(t => t.Id == ticketId);
        ticket.Status = status;
        await db.SaveChangesAsync();
    }

    private static async Task<HttpClient> CreateAuthenticatedClientAsync(TicketCommentApiFactory factory, string email)
    {
        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = OpsSphereSeedData.LocalDemoPassword
        });
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await ReadResponseAsync<ApiResponse<LoginData>>(loginResponse);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Data.AccessToken);
        return client;
    }

    private static async Task<T> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var envelope = await ReadResponseAsync<ApiResponse<T>>(response);
        return envelope.Data;
    }

    private static async Task<ApiErrorResponse> ReadErrorAsync(HttpResponseMessage response) =>
        await ReadResponseAsync<ApiErrorResponse>(response);

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name} from {(int)response.StatusCode}: {json}");
    }

    private sealed record ApiResponse<T>(T Data);
    private sealed record ApiErrorResponse(ApiError Error);
    private sealed record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail>? Details, string? CorrelationId);
    private sealed record ApiErrorDetail(string? Field, string Message);
    private sealed record LoginData(string AccessToken);

    private sealed record CreateTicketResult(
        Guid Id,
        string TicketNumber,
        string Status,
        string Priority,
        string SlaState,
        DateTime? SlaDueAt);

    private sealed record AddInternalCommentResponse(
        Guid CommentId,
        Guid TicketId,
        string TicketNumber,
        Guid AuthorUserId,
        string AuthorDisplayName,
        string Body,
        DateTime CreatedAt,
        string Message);

    private sealed record TicketCommentDto(
        Guid Id,
        Guid TicketId,
        Guid AuthorUserId,
        string AuthorDisplayName,
        string Body,
        DateTime CreatedAt);

    internal sealed class TicketCommentApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtSigningKey = "integration-testing-only-fictional-jwt-signing-key";

        private readonly SqliteConnection connection = new("Data Source=:memory:");

        public static async Task<TicketCommentApiFactory> CreateAsync()
        {
            ConfigureEnvironment();
            var factory = new TicketCommentApiFactory();
            await factory.connection.OpenAsync();
            await factory.InitializeDatabaseAsync();
            return factory;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=OpsSphereTicketCommentTests;Trusted_Connection=True;TrustServerCertificate=True;",
                    ["SeedData:Enabled"] = "false",
                    ["Jwt:Issuer"] = "OpsSphere.Tests",
                    ["Jwt:Audience"] = "OpsSphere.Tests.Angular",
                    ["Jwt:ExpirationMinutes"] = "60",
                    ["Jwt:SigningKey"] = JwtSigningKey
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<OpsSphereDbContext>>();
                services.RemoveAll<DbContextOptions<OpsSphereDbContext>>();
                services.AddDbContext<OpsSphereDbContext>(options => options.UseSqlite(connection));
            });
        }

        private static void ConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(local);Database=OpsSphereTicketCommentTests;Trusted_Connection=True;TrustServerCertificate=True;");
            Environment.SetEnvironmentVariable("SeedData__Enabled", "false");
            Environment.SetEnvironmentVariable("Jwt__Issuer", "OpsSphere.Tests");
            Environment.SetEnvironmentVariable("Jwt__Audience", "OpsSphere.Tests.Angular");
            Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
            Environment.SetEnvironmentVariable("Jwt__SigningKey", JwtSigningKey);
        }

        public override async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
            await base.DisposeAsync();
        }

        private async Task InitializeDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OpsSphereDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<OpsSphereDataSeeder>();
            await seeder.SeedAsync();
        }
    }
}
