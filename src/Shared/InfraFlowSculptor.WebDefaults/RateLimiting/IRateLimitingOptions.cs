namespace InfraFlowSculptor.WebDefaults.RateLimiting;

/// <summary>
/// Contract for rate-limiting options that carry a Global and Expensive policy pair.
/// </summary>
public interface IRateLimitingOptions
{
    /// <summary>Gets the global policy applied to every incoming request.</summary>
    FixedWindowRateLimitingPolicyOptions Global { get; }

    /// <summary>Gets the stricter policy applied to expensive endpoints.</summary>
    FixedWindowRateLimitingPolicyOptions Expensive { get; }
}
