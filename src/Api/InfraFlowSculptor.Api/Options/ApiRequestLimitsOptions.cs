using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Api.Options;

/// <summary>
/// Represents the API request body size limits configuration.
/// </summary>
public sealed class ApiRequestLimitsOptions
{
    /// <summary>Gets the configuration section name.</summary>
    public const string SectionName = "RequestLimits";

    /// <summary>Gets the default max request body size in bytes.</summary>
    public const long DefaultMaxRequestBodySizeBytes = 52_428_800;

    /// <summary>Gets or sets the maximum accepted request body size in bytes.</summary>
    [Range(1, long.MaxValue)]
    public long MaxRequestBodySizeBytes { get; set; } = DefaultMaxRequestBodySizeBytes;
}
