using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Identity.Web;

namespace InfraFlowSculptor.WebDefaults.RateLimiting;

/// <summary>
/// Shared rate-limiting infrastructure: partition key resolution and limiter factory helpers.
/// </summary>
public static class RateLimitingHelper
{
    private const string AnonymousIpPartitionPrefix = "ip:";
    private const string AuthenticatedUserPartitionPrefix = "user:";
    private const string UnknownIpPartitionKey = "unknown";

    /// <summary>
    /// Resolves the partition key: authenticated traffic is partitioned by user identity;
    /// anonymous traffic is partitioned by remote IP address.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The partition key string.</returns>
    public static string ResolvePartitionKey(HttpContext httpContext)
    {
        var userId = httpContext.User?.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirst(ClaimConstants.ObjectId)?.Value
                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            : null;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return string.Concat(AuthenticatedUserPartitionPrefix, userId);
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownIpPartitionKey;
        return string.Concat(AnonymousIpPartitionPrefix, ip);
    }

    /// <summary>Creates a partitioned fixed-window limiter keyed by user/IP.</summary>
    /// <param name="policyOptions">The fixed-window policy options.</param>
    /// <returns>The partitioned rate limiter.</returns>
    public static PartitionedRateLimiter<HttpContext> CreateFixedWindowPartitionedLimiter(
        FixedWindowRateLimitingPolicyOptions policyOptions)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ResolvePartitionKey(httpContext),
                factory: _ => CreateFixedWindowRateLimiterOptions(policyOptions)));
    }

    /// <summary>Creates a <see cref="FixedWindowRateLimiterOptions"/> from the policy options.</summary>
    /// <param name="policyOptions">The policy options to convert.</param>
    /// <returns>The configured rate limiter options.</returns>
    public static FixedWindowRateLimiterOptions CreateFixedWindowRateLimiterOptions(
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

    /// <summary>Applies the standard 429 rejection handler with Retry-After header.</summary>
    /// <param name="rateLimiterOptions">The rate limiter options to configure.</param>
    public static void ConfigureRejectionHandler(RateLimiterOptions rateLimiterOptions)
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
    }
}
