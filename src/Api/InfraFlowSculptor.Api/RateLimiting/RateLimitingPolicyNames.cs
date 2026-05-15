namespace InfraFlowSculptor.Api.RateLimiting;

/// <summary>
/// Defines the named ASP.NET Core rate-limiting policies used by the API.
/// </summary>
public static class RateLimitingPolicyNames
{
    /// <summary>Gets the policy name applied to expensive generation and push endpoints.</summary>
    public const string Expensive = "Expensive";

    /// <summary>Gets the policy name applied to health-check endpoints.</summary>
    public const string HealthChecks = "HealthChecks";
}