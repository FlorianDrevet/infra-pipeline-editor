using InfraFlowSculptor.Mcp.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace InfraFlowSculptor.Mcp.Common;

/// <summary>
/// Maps the MCP host health endpoints with the repository's expected anonymity and rate-limiting settings.
/// </summary>
public static class HealthChecksEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the standard MCP health and liveness endpoints.
    /// </summary>
    /// <param name="application">The web application to configure.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
    public static WebApplication MapMcpHealthChecks(this WebApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.MapHealthChecks("/health")
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingPolicyNames.HealthChecks);

        application.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live"),
            })
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingPolicyNames.HealthChecks);

        return application;
    }
}