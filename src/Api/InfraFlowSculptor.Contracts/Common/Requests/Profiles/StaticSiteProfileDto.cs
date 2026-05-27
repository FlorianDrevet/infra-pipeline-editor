using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests.Profiles;

/// <summary>
/// Pipeline profile configuration for static site builds.
/// </summary>
public sealed class StaticSiteProfileDto : PipelineStackProfileDto
{
    /// <summary>Build command for the static site (e.g. <c>npm run build</c>).</summary>
    [MaxLength(500)]
    public string? BuildCommand { get; init; }

    /// <summary>Output directory of the build (e.g. <c>dist</c>).</summary>
    [MaxLength(200)]
    public string? OutputDirectory { get; init; }
}
