using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace InfraFlowSculptor.Api.RateLimiting;

/// <summary>
/// Registers global and named rate-limiting policies used across the API.
/// Audit SEC-003 (2026-04-23): the previous configuration only protected the
/// Login endpoint; expensive generation endpoints are now bounded by the
/// <see cref="ExpensivePolicy"/> partition while every authenticated user
/// (or anonymous IP) is bounded by the global limiter.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Name of the rate-limiting policy applied to the Login endpoint.</summary>
    public const string LoginPolicy = "Login";

    /// <summary>Name of the rate-limiting policy applied to expensive generation endpoints.</summary>
    public const string ExpensivePolicy = "Expensive";

    private const int GlobalPermitsPerMinute = 100;
    private const int ExpensivePermitsPerMinute = 10;

    /// <summary>Adds the rate-limiting middleware configuration to the DI container.</summary>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = GlobalPermitsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }));

            options.AddFixedWindowLimiter(LoginPolicy, opt =>
            {
                opt.Window = TimeSpan.FromSeconds(10);
                opt.PermitLimit = 3;
            });

            options.AddPolicy(ExpensivePolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = ExpensivePermitsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }));
        });
        return services;
    }

    /// <summary>
    /// Resolves the partition key used by the rate limiter — authenticated users are
    /// partitioned by their stable identity claim, anonymous traffic by remote IP.
    /// </summary>
    private static string ResolvePartitionKey(HttpContext httpContext)
    {
        var userId = httpContext.User?.Identity?.IsAuthenticated == true
            ? httpContext.User.Identity.Name
            : null;

        if (!string.IsNullOrWhiteSpace(userId))
            return $"user:{userId}";

        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        return $"ip:{ip ?? "unknown"}";
    }
}
