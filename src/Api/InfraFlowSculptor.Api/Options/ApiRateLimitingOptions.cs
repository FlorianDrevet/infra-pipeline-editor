using InfraFlowSculptor.WebDefaults.RateLimiting;

namespace InfraFlowSculptor.Api.Options;

/// <summary>
/// Represents the native ASP.NET Core rate-limiting configuration used by the API.
/// </summary>
public sealed class ApiRateLimitingOptions : IRateLimitingOptions
{
    /// <summary>Gets the configuration section name.</summary>
    public const string SectionName = "RateLimiting";

    private static readonly FixedWindowRateLimitingPolicyOptions DefaultHealthChecksPolicy = new()
    {
        PermitLimit = 30,
        WindowSeconds = 60,
        QueueLimit = 0,
    };

    /// <summary>Gets or sets the global policy applied to every incoming request.</summary>
    public FixedWindowRateLimitingPolicyOptions Global { get; set; } = new();

    /// <summary>Gets or sets the stricter policy applied to expensive endpoints.</summary>
    public FixedWindowRateLimitingPolicyOptions Expensive { get; set; } = new();

    /// <summary>Gets or sets the policy applied to the health-check endpoints.</summary>
    public FixedWindowRateLimitingPolicyOptions HealthChecks { get; set; } = DefaultHealthChecksPolicy;
}
