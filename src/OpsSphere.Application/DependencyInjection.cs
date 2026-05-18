using Microsoft.Extensions.DependencyInjection;
using OpsSphere.Application.Features.Auth.GetCurrentUser;
using OpsSphere.Application.Features.Auth.Login;
using OpsSphere.Application.Features.CustomerManagement;
using OpsSphere.Application.Features.OrganizationManagement;
using OpsSphere.Application.Features.SlaManagement;
using OpsSphere.Application.Features.TicketManagement;
using OpsSphere.Application.Features.UserManagement;
using OpsSphere.Domain.Services;

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
        services.AddScoped<GetRegionsQueryHandler>();
        services.AddScoped<GetRegionByIdQueryHandler>();
        services.AddScoped<CreateRegionCommandHandler>();
        services.AddScoped<UpdateRegionCommandHandler>();
        services.AddScoped<DeactivateRegionCommandHandler>();
        services.AddScoped<GetCountriesQueryHandler>();
        services.AddScoped<GetCountryByIdQueryHandler>();
        services.AddScoped<CreateCountryCommandHandler>();
        services.AddScoped<UpdateCountryCommandHandler>();
        services.AddScoped<DeactivateCountryCommandHandler>();
        services.AddScoped<GetAccountsQueryHandler>();
        services.AddScoped<GetAccountByIdQueryHandler>();
        services.AddScoped<CreateAccountCommandHandler>();
        services.AddScoped<UpdateAccountCommandHandler>();
        services.AddScoped<DeactivateAccountCommandHandler>();
        services.AddScoped<GetCampaignsQueryHandler>();
        services.AddScoped<GetCampaignByIdQueryHandler>();
        services.AddScoped<CreateCampaignCommandHandler>();
        services.AddScoped<UpdateCampaignCommandHandler>();
        services.AddScoped<DeactivateCampaignCommandHandler>();
        services.AddScoped<GetUserScopesQueryHandler>();
        services.AddScoped<UpdateUserScopesCommandHandler>();
        services.AddScoped<GetCustomersQueryHandler>();
        services.AddScoped<GetCustomerByIdQueryHandler>();
        services.AddScoped<CreateCustomerCommandHandler>();
        services.AddScoped<UpdateCustomerCommandHandler>();
        services.AddScoped<DeactivateCustomerCommandHandler>();
        services.AddScoped<GetCustomerTicketsQueryHandler>();
        services.AddScoped<CreateTicketCommandHandler>();
        services.AddScoped<GetTicketsQueryHandler>();
        services.AddScoped<GetTicketByIdQueryHandler>();
        services.AddScoped<AssignTicketCommandHandler>();
        services.AddScoped<GetEligibleAgentsQueryHandler>();
        services.AddScoped<UpdateTicketStatusCommandHandler>();
        services.AddScoped<UpdateTicketPriorityCommandHandler>();
        services.AddScoped<AddInternalCommentCommandHandler>();
        services.AddScoped<GetTicketCommentsQueryHandler>();
        services.AddScoped<EscalateTicketCommandHandler>();
        services.AddScoped<GetEscalationQueueQueryHandler>();
        services.AddScoped<ResolveTicketCommandHandler>();
        services.AddScoped<CloseTicketCommandHandler>();
        services.AddScoped<GetTicketStatusHistoryQueryHandler>();
        services.AddScoped<GetSlaSummaryQueryHandler>();
        services.AddSingleton<SlaEvaluator>();

        return services;
    }
}
