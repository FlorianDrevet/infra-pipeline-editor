using InfraFlowSculptor.WebDefaults.RateLimiting;
using Microsoft.Extensions.Options;

namespace InfraFlowSculptor.Mcp.RateLimiting;

/// <summary>Validates <see cref="McpRateLimitingOptions"/> at startup.</summary>
internal sealed class McpRateLimitingOptionsValidator : RateLimitingOptionsValidator<McpRateLimitingOptions>;
