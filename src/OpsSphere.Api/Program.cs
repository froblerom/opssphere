using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using OpsSphere.Api.Authorization;
using OpsSphere.Api.Common;
using OpsSphere.Api.Health;
using OpsSphere.Api.Middleware;
using OpsSphere.Application;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Authorization;
using OpsSphere.Infrastructure;
using OpsSphere.Infrastructure.Authentication;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = ReadJwtOptions(builder.Configuration);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<ConfigurationHealthCheck>("configuration");
builder.Services.AddAuthorization(options =>
{
    // Register one named policy per permission code — policy name equals permission code
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

var app = builder.Build();

var seedDataEnabled = bool.TryParse(app.Configuration[$"{SeedDataOptions.SectionName}:Enabled"], out var parsedSeedDataEnabled) &&
    parsedSeedDataEnabled;

if (SeedDataStartupGate.ShouldRun(app.Environment.EnvironmentName, seedDataEnabled))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<OpsSphereDataSeeder>();
    await seeder.SeedAsync();
}

// Correlation ID first — ensures every subsequent middleware and log has the correlation ID
app.UseMiddleware<CorrelationIdMiddleware>();

// Global exception handling after correlation ID so error responses include correlation ID
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Serilog request logging — enriches logs with CorrelationId and UserId from HttpContext
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var correlationId = httpContext.Items[CorrelationIdMiddleware.ItemsKey] as string;
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            diagnosticContext.Set("CorrelationId", correlationId);
        }

        var userId = httpContext.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});

app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteSimpleAsync,
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/details", new HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteDetailsAsync,
    AllowCachingResponses = false
});

app.Run();

static JwtOptions ReadJwtOptions(IConfiguration configuration)
{
    var options = new JwtOptions
    {
        Issuer = configuration[$"{JwtOptions.SectionName}:Issuer"] ?? string.Empty,
        Audience = configuration[$"{JwtOptions.SectionName}:Audience"] ?? string.Empty,
        SigningKey = configuration[$"{JwtOptions.SectionName}:SigningKey"] ?? string.Empty,
        ExpirationMinutes = int.TryParse(configuration[$"{JwtOptions.SectionName}:ExpirationMinutes"], out var expirationMinutes)
            ? expirationMinutes
            : 60
    };

    if (string.IsNullOrWhiteSpace(options.Issuer))
    {
        throw new InvalidOperationException("Jwt:Issuer is not configured.");
    }

    if (string.IsNullOrWhiteSpace(options.Audience))
    {
        throw new InvalidOperationException("Jwt:Audience is not configured.");
    }

    if (string.IsNullOrWhiteSpace(options.SigningKey))
    {
        throw new InvalidOperationException("Jwt:SigningKey is not configured.");
    }

    if (Encoding.UTF8.GetByteCount(options.SigningKey) < 32)
    {
        throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes for local JWT validation.");
    }

    return options;
}

public partial class Program
{
}
