using System.Threading.RateLimiting;
using InfraFlowSculptor.Api.Options;
using InfraFlowSculptor.WebDefaults.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Api.RateLimiting;

/// <summary>
/// Registers the native ASP.NET Core rate-limiting policies used by the API.
/// </summary>
public static class RateLimitingServiceCollectionExtensions
{
    /// <summary>
    /// Adds the configured rate-limiting middleware and validates the bound configuration at startup.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddOptions<ApiRateLimitingOptions>()
            .BindConfiguration(ApiRateLimitingOptions.SectionName)
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ApiRateLimitingOptions>, ApiRateLimitingOptionsValidator>();

        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<ApiRateLimitingOptions>>((rateLimiterOptions, apiRateLimitingOptions) =>
                ConfigureRateLimiterOptions(rateLimiterOptions, apiRateLimitingOptions.Value));

        return services;
    }

    private static void ConfigureRateLimiterOptions(
        RateLimiterOptions rateLimiterOptions,
        ApiRateLimitingOptions apiRateLimitingOptions)
    {
        RateLimitingHelper.ConfigureRejectionHandler(rateLimiterOptions);

        rateLimiterOptions.GlobalLimiter = RateLimitingHelper.CreateFixedWindowPartitionedLimiter(apiRateLimitingOptions.Global);

        rateLimiterOptions.AddPolicy(
            RateLimitingPolicyNames.Expensive,
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: RateLimitingHelper.ResolvePartitionKey(httpContext),
                factory: _ => RateLimitingHelper.CreateFixedWindowRateLimiterOptions(apiRateLimitingOptions.Expensive)));

        rateLimiterOptions.AddPolicy(
            RateLimitingPolicyNames.HealthChecks,
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: RateLimitingHelper.ResolvePartitionKey(httpContext),
                factory: _ => RateLimitingHelper.CreateFixedWindowRateLimiterOptions(apiRateLimitingOptions.HealthChecks)));
    }
}
