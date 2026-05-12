using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using InfraFlowSculptor.Api.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace InfraFlowSculptor.Api.RateLimiting;

/// <summary>
/// Registers the native ASP.NET Core rate-limiting policies used by the API.
/// </summary>
public static class RateLimitingServiceCollectionExtensions
{
    private const string AnonymousIpPartitionPrefix = "ip:";
    private const string AuthenticatedUserPartitionPrefix = "user:";
    private const string UnknownIpPartitionKey = "unknown";

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
        rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        rateLimiterOptions.OnRejected = static (context, _) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            {
                var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
            }

            return ValueTask.CompletedTask;
        };

        rateLimiterOptions.GlobalLimiter = CreateFixedWindowPartitionedLimiter(apiRateLimitingOptions.Global);

        rateLimiterOptions.AddPolicy(
            RateLimitingPolicyNames.Expensive,
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ResolvePartitionKey(httpContext),
                factory: _ => CreateFixedWindowRateLimiterOptions(apiRateLimitingOptions.Expensive)));
    }

    private static PartitionedRateLimiter<HttpContext> CreateFixedWindowPartitionedLimiter(
        FixedWindowRateLimitingPolicyOptions policyOptions)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ResolvePartitionKey(httpContext),
                factory: _ => CreateFixedWindowRateLimiterOptions(policyOptions)));
    }

    private static FixedWindowRateLimiterOptions CreateFixedWindowRateLimiterOptions(
        FixedWindowRateLimitingPolicyOptions policyOptions)
    {
        return new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = policyOptions.PermitLimit,
            QueueLimit = policyOptions.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            Window = TimeSpan.FromSeconds(policyOptions.WindowSeconds),
        };
    }

    /// <summary>
    /// Resolves the partition key used by the rate limiter so authenticated traffic is partitioned by user identity and anonymous traffic by remote IP address.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The partition key used by the rate limiter.</returns>
    private static string ResolvePartitionKey(HttpContext httpContext)
    {
        var userId = httpContext.User?.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirst(ClaimConstants.ObjectId)?.Value
                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? httpContext.User.Identity?.Name
            : null;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return string.Concat(AuthenticatedUserPartitionPrefix, userId);
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownIpPartitionKey;
        return string.Concat(AnonymousIpPartitionPrefix, ip);
    }
}