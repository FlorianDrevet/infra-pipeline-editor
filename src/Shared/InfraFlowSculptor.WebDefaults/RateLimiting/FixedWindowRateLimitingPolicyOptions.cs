namespace InfraFlowSculptor.WebDefaults.RateLimiting;

/// <summary>
/// Represents the configurable values for a fixed-window rate-limiting policy.
/// </summary>
public sealed class FixedWindowRateLimitingPolicyOptions
{
    /// <summary>Gets or sets the number of permits available during a single window.</summary>
    public int PermitLimit { get; set; }

    /// <summary>Gets or sets the window duration in seconds.</summary>
    public int WindowSeconds { get; set; }

    /// <summary>Gets or sets the number of queued requests allowed for the policy.</summary>
    public int QueueLimit { get; set; }
}
