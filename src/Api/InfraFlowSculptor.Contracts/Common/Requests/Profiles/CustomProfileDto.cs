using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests.Profiles;

/// <summary>
/// Pipeline profile configuration for custom/advanced pipelines with raw commands.
/// </summary>
public sealed class CustomProfileDto : PipelineStackProfileDto
{
    /// <summary>Custom test command.</summary>
    [MaxLength(500)]
    public string? CustomTestCommand { get; init; }

    /// <summary>Custom lint command.</summary>
    [MaxLength(500)]
    public string? CustomLintCommand { get; init; }

    /// <summary>Custom build command.</summary>
    [MaxLength(500)]
    public string? CustomBuildCommand { get; init; }
}
