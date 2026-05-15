using System.Net.Mime;
using System.Text.Json;
using OpsSphere.Api.Common;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Api.Middleware;

public sealed class ActiveUserMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var authorizationService = context.RequestServices.GetRequiredService<ICurrentUserAuthorizationService>();
            var profile = await authorizationService.GetCurrentUserAuthorizationAsync(context.RequestAborted);
            if (profile is null || !profile.IsActive)
            {
                var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey] as string;
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = MediaTypeNames.Application.Json;

                var envelope = new ApiErrorEnvelope(new ApiError(
                    ErrorCodes.Unauthorized,
                    "Authentication is required.",
                    CorrelationId: correlationId));

                await context.Response.WriteAsync(JsonSerializer.Serialize(envelope, JsonOptions), context.RequestAborted);
                return;
            }
        }

        await next(context);
    }
}
