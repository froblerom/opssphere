using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Infrastructure.Auditing;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Authentication;
using OpsSphere.Infrastructure.Authorization;
using OpsSphere.Infrastructure.Identity;
using OpsSphere.Infrastructure.CustomerManagement;
using OpsSphere.Infrastructure.OrganizationManagement;
using OpsSphere.Infrastructure.SlaManagement;
using OpsSphere.Infrastructure.TicketManagement;
using OpsSphere.Infrastructure.Persistence;
using OpsSphere.Infrastructure.Persistence.SeedData;
using OpsSphere.Infrastructure.UserManagement;

namespace OpsSphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<OpsSphereDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<SeedDataOptions>(options =>
            options.Enabled = bool.TryParse(configuration[$"{SeedDataOptions.SectionName}:Enabled"], out var enabled) && enabled);
        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = configuration[$"{JwtOptions.SectionName}:Issuer"] ?? string.Empty;
            options.Audience = configuration[$"{JwtOptions.SectionName}:Audience"] ?? string.Empty;
            options.SigningKey = configuration[$"{JwtOptions.SectionName}:SigningKey"] ?? string.Empty;
            options.ExpirationMinutes = int.TryParse(configuration[$"{JwtOptions.SectionName}:ExpirationMinutes"], out var expirationMinutes)
                ? expirationMinutes
                : 60;
        });

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IPasswordVerifier, PasswordVerifier>();
        services.AddScoped<IAuthUserReader, AuthUserReader>();
        services.AddScoped<IAuthUnitOfWork, AuthUnitOfWork>();
        services.AddScoped<IUserManagementRepository, UserManagementRepository>();
        services.AddScoped<IOrganizationManagementRepository, OrganizationManagementRepository>();
        services.AddScoped<ICustomerManagementRepository, CustomerManagementRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ISlaRepository, SlaRepository>();
        services.AddScoped<ISlaSummaryRepository, SlaSummaryRepository>();
        services.AddScoped<ITicketNumberGenerator, SequentialTicketNumberGenerator>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<OpsSphereDataSeeder>();

        services.AddScoped<ICurrentUserAuthorizationService, CurrentUserAuthorizationService>();
        services.AddScoped<IScopeAuthorizationService, ScopeAuthorizationService>();

        return services;
    }
}
