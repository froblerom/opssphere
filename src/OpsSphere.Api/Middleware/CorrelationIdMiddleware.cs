using System.Text.RegularExpressions;

namespace OpsSphere.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemsKey = "CorrelationId";

    private const int MaxLength = 100;
    private static readonly Regex SafePattern = new(@"^[a-zA-Z0-9\-_\.]+$", RegexOptions.Compiled);

    private readonly RequestDelegate next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[ItemsKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            var incoming = values.FirstOrDefault();
            if (IsValid(incoming))
            {
                return incoming!;
            }
        }

        return Guid.NewGuid().ToString("N");
    }

    private static bool IsValid(string? id) =>
        !string.IsNullOrWhiteSpace(id) &&
        id.Length <= MaxLength &&
        SafePattern.IsMatch(id);
}
