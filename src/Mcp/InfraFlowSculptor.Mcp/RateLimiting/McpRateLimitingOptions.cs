namespace InfraFlowSculptor.Mcp.RateLimiting;

/// <summary>
/// Represents the ASP.NET Core rate-limiting configuration used by the MCP host.
/// </summary>
public sealed class McpRateLimitingOptions
{
    /// <summary>Gets the configuration section name.</summary>
    public const string SectionName = "RateLimiting";

    /// <summary>Gets or sets the global policy applied to every incoming request.</summary>
    public FixedWindowRateLimitingPolicyOptions Global { get; set; } = new();

    /// <summary>Gets or sets the stricter policy applied to expensive endpoints.</summary>
    public FixedWindowRateLimitingPolicyOptions Expensive { get; set; } = new();
}