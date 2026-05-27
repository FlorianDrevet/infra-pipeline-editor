using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests.Profiles;

/// <summary>
/// Pipeline profile configuration for Angular applications.
/// </summary>
public sealed class AngularProfileDto : PipelineStackProfileDto
{
    /// <summary>Package manager: Npm, Yarn, or Pnpm.</summary>
    [MaxLength(20)]
    public string? PackageManager { get; init; }

    /// <summary>Whether to run <c>ng test</c>.</summary>
    public bool RunNgTest { get; init; } = true;

    /// <summary>Whether to run <c>ng lint</c>.</summary>
    public bool RunNgLint { get; init; }

    /// <summary>Whether to run <c>ng build --configuration production</c>.</summary>
    public bool RunNgBuildProduction { get; init; } = true;

    /// <summary>Optional Angular project name for multi-project workspaces.</summary>
    [MaxLength(200)]
    public string? ProjectName { get; init; }
}
