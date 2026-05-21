namespace InfraFlowSculptor.Contracts.Common;

/// <summary>
/// Response DTO containing pipeline options auto-detected from the project's source repository.
/// </summary>
public sealed record DetectedPipelineOptionsResponse
{
    /// <summary>Detected test framework identifier (xunit, nunit, jest, vitest, pytest, etc.).</summary>
    public string? TestFramework { get; init; }

    /// <summary>Suggested test command based on the detected framework.</summary>
    public string? SuggestedTestCommand { get; init; }

    /// <summary>Suggested test results format (VSTest or JUnit).</summary>
    public string? SuggestedTestResultsFormat { get; init; }

    /// <summary>Suggested coverage tool (Cobertura or JaCoCo).</summary>
    public string? SuggestedCoverageTool { get; init; }

    /// <summary>Suggested path to the coverage report file.</summary>
    public string? SuggestedCoverageReportPath { get; init; }

    /// <summary>Whether linting tools were detected in the repository.</summary>
    public bool LintingAvailable { get; init; }

    /// <summary>Suggested lint command based on detected linting tools.</summary>
    public string? SuggestedLintCommand { get; init; }

    /// <summary>Whether a Sonar configuration was detected in the repository.</summary>
    public bool SonarConfigDetected { get; init; }

    /// <summary>Suggested Sonar project key if configuration was found.</summary>
    public string? SuggestedSonarProjectKey { get; init; }

    /// <summary>Whether dependency scanning tools are available.</summary>
    public bool DependencyScanAvailable { get; init; }

    /// <summary>Suggested dependency scan tool identifier.</summary>
    public string? SuggestedDependencyScanTool { get; init; }
}
