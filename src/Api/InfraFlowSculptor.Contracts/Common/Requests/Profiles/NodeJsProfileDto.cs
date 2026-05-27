using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests.Profiles;

/// <summary>
/// Pipeline profile configuration for Node.js applications.
/// </summary>
public sealed class NodeJsProfileDto : PipelineStackProfileDto
{
    /// <summary>Package manager: Npm, Yarn, or Pnpm.</summary>
    [MaxLength(20)]
    public string? PackageManager { get; init; }

    /// <summary>Test framework: Jest, Vitest, or Mocha.</summary>
    [MaxLength(20)]
    public string? TestFramework { get; init; }

    /// <summary>Whether to run the lint script.</summary>
    public bool RunLintScript { get; init; }

    /// <summary>Name of the test script in package.json.</summary>
    [MaxLength(100)]
    public string? TestScriptName { get; init; }

    /// <summary>Name of the lint script in package.json.</summary>
    [MaxLength(100)]
    public string? LintScriptName { get; init; }
}
