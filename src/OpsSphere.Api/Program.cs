using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OpsSphere.Api.Authorization;
using OpsSphere.Application;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Authorization;
using OpsSphere.Infrastructure;
using OpsSphere.Infrastructure.Authentication;
using OpsSphere.Infrastructure.Persistence.SeedData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
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
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
