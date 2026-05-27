using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests.Profiles;

/// <summary>
/// Pipeline profile configuration for .NET applications.
/// </summary>
public sealed class DotNetProfileDto : PipelineStackProfileDto
{
    /// <summary>Test framework: XUnit, NUnit, or MSTest.</summary>
    [MaxLength(20)]
    public string? TestFramework { get; init; }

    /// <summary>Whether to collect code coverage during test runs.</summary>
    public bool CollectCoverage { get; init; }

    /// <summary>Optional glob pattern to locate test projects.</summary>
    [MaxLength(200)]
    public string? CustomTestProjectGlob { get; init; }
}
