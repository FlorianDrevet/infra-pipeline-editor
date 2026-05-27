using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests.Profiles;

/// <summary>
/// Pipeline profile configuration for Python applications.
/// </summary>
public sealed class PythonProfileDto : PipelineStackProfileDto
{
    /// <summary>Package manager: Pip, Poetry, or Uv.</summary>
    [MaxLength(20)]
    public string? PackageManager { get; init; }

    /// <summary>Test framework: Pytest or Unittest.</summary>
    [MaxLength(20)]
    public string? TestFramework { get; init; }

    /// <summary>Whether to collect code coverage.</summary>
    public bool CollectCoverage { get; init; }
}
