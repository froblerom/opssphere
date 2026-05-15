using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Application.Features.Auth.GetCurrentUser;
using OpsSphere.Application.Features.Auth.Login;
using OpsSphere.Application.Features.UserManagement;

namespace OpsSphere.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<GetCurrentUserQueryHandler>();
        services.AddScoped<GetUsersQueryHandler>();
        services.AddScoped<GetUserByIdQueryHandler>();
        services.AddScoped<CreateUserCommandHandler>();
        services.AddScoped<UpdateUserCommandHandler>();
        services.AddScoped<DeactivateUserCommandHandler>();
        services.AddScoped<UpdateUserRolesCommandHandler>();
        services.AddScoped<GetRolesQueryHandler>();
        services.AddScoped<GetPermissionsQueryHandler>();

        return services;
    }
}
