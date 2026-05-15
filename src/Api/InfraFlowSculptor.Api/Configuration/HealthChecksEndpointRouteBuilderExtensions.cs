using InfraFlowSculptor.Api.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace InfraFlowSculptor.Api.Configuration;

/// <summary>
/// Maps the API health-check endpoints.
/// </summary>
public static class HealthChecksEndpointRouteBuilderExtensions
{
    private const string HealthEndpointPattern = "/health";
    private const string LivenessEndpointPattern = "/alive";

    /// <summary>
    /// Maps the API health-check endpoints with the dedicated health-check rate-limiting policy.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapApiHealthChecks(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);

        endpointRouteBuilder.MapHealthChecks(HealthEndpointPattern)
            .RequireRateLimiting(RateLimitingPolicyNames.HealthChecks);

        endpointRouteBuilder.MapHealthChecks(LivenessEndpointPattern, new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live"),
            })
            .RequireRateLimiting(RateLimitingPolicyNames.HealthChecks);

        return endpointRouteBuilder;
    }
}