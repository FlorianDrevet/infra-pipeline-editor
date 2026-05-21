using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests.Profiles;

/// <summary>
/// Pipeline profile configuration for Java applications.
/// </summary>
public sealed class JavaProfileDto : PipelineStackProfileDto
{
    /// <summary>Build tool: Maven or Gradle.</summary>
    [MaxLength(20)]
    public string? BuildTool { get; init; }

    /// <summary>Test framework: JUnit5, JUnit4, or TestNg.</summary>
    [MaxLength(20)]
    public string? TestFramework { get; init; }

    /// <summary>Whether to collect code coverage (JaCoCo).</summary>
    public bool CollectCoverage { get; init; }
}
