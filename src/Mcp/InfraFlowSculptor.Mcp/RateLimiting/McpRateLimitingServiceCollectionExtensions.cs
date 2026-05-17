using System.Threading.RateLimiting;
using InfraFlowSculptor.WebDefaults.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Mcp.RateLimiting;

/// <summary>
/// Registers the ASP.NET Core rate-limiting policies used by the MCP host.
/// </summary>
public static class McpRateLimitingServiceCollectionExtensions
{
    /// <summary>
    /// Adds the MCP rate-limiting services and validates the bound configuration at startup.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMcpRateLimiting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddOptions<McpRateLimitingOptions>()
            .BindConfiguration(McpRateLimitingOptions.SectionName)
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<McpRateLimitingOptions>, McpRateLimitingOptionsValidator>();

        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<McpRateLimitingOptions>>((rateLimiterOptions, mcpRateLimitingOptions) =>
                ConfigureRateLimiterOptions(rateLimiterOptions, mcpRateLimitingOptions.Value));

        return services;
    }

    private static void ConfigureRateLimiterOptions(
        RateLimiterOptions rateLimiterOptions,
        McpRateLimitingOptions mcpRateLimitingOptions)
    {
        RateLimitingHelper.ConfigureRejectionHandler(rateLimiterOptions);

        rateLimiterOptions.GlobalLimiter = RateLimitingHelper.CreateFixedWindowPartitionedLimiter(mcpRateLimitingOptions.Global);

        rateLimiterOptions.AddPolicy(
            RateLimitingPolicyNames.Expensive,
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: RateLimitingHelper.ResolvePartitionKey(httpContext),
                factory: _ => RateLimitingHelper.CreateFixedWindowRateLimiterOptions(mcpRateLimitingOptions.Expensive)));
    }
}
