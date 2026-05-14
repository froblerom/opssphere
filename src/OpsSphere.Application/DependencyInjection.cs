using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Application.Features.Auth.GetCurrentUser;
using OpsSphere.Application.Features.Auth.Login;

namespace OpsSphere.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<GetCurrentUserQueryHandler>();

        return services;
    }
}
