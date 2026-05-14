using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OpsSphere.Api.Health;

public sealed class ConfigurationHealthCheck : IHealthCheck
{
    private static readonly string[] RequiredKeys =
    [
        "ConnectionStrings:DefaultConnection",
        "Jwt:Issuer",
        "Jwt:Audience",
        "Jwt:SigningKey"
    ];

    private readonly IConfiguration configuration;

    public ConfigurationHealthCheck(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var missing = RequiredKeys
            .Where(key => string.IsNullOrWhiteSpace(configuration[key]))
            .ToList();

        // Report missing key names only — never the values (they may be secrets)
        var result = missing.Count == 0
            ? HealthCheckResult.Healthy("All required configuration keys are present.")
            : HealthCheckResult.Unhealthy($"Missing required configuration: {string.Join(", ", missing)}");

        return Task.FromResult(result);
    }
}
