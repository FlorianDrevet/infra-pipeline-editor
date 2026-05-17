using InfraFlowSculptor.WebDefaults.RateLimiting;

namespace InfraFlowSculptor.Mcp.RateLimiting;

/// <summary>Validates <see cref="McpRateLimitingOptions"/> at startup.</summary>
internal sealed class McpRateLimitingOptionsValidator : RateLimitingOptionsValidator<McpRateLimitingOptions>;
