namespace InfraFlowSculptor.Mcp.RateLimiting;

/// <summary>
/// Defines the named ASP.NET Core rate-limiting policies used by the MCP host.
/// </summary>
public static class RateLimitingPolicyNames
{
    /// <summary>Gets the policy name applied to expensive MCP endpoints.</summary>
    public const string Expensive = "Expensive";

    /// <summary>Gets the policy name applied to MCP health-check endpoints.</summary>
    public const string HealthChecks = "HealthChecks";
}
