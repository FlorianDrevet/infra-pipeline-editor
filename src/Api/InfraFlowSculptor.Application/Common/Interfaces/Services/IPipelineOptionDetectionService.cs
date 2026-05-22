using ErrorOr;

namespace InfraFlowSculptor.Application.Common.Interfaces.Services;

/// <summary>
/// Analyzes the Git repository content of a compute resource to auto-detect
/// test frameworks, linting tools, and other CI/CD pipeline options.
/// </summary>
public interface IPipelineOptionDetectionService
{
    /// <summary>
    /// Analyzes the repository for the given runtime stack and source path to detect available pipeline options.
    /// </summary>
    /// <param name="gitProvider">The resolved Git provider service to use for repository access.</param>
    /// <param name="token">Personal access token for repository access.</param>
    /// <param name="owner">Repository owner (org/user or "org/project" for Azure DevOps).</param>
    /// <param name="repositoryName">Repository name.</param>
    /// <param name="branch">Branch to analyze.</param>
    /// <param name="runtimeStack">The runtime stack identifier (DotNet, Node, Python, Java).</param>
    /// <param name="sourceCodePath">Optional sub-path to the source code within the repository.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detected pipeline options, or an error if repository access fails.</returns>
    Task<ErrorOr<DetectedPipelineOptionsResult>> DetectAsync(
        IGitProviderService gitProvider,
        string token,
        string owner,
        string repositoryName,
        string branch,
        string runtimeStack,
        string? sourceCodePath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of auto-detection containing suggested pipeline options.
/// </summary>
public sealed record DetectedPipelineOptionsResult
{
    /// <summary>Detected test framework identifier.</summary>
    public string? TestFramework { get; init; }

    /// <summary>Suggested test command.</summary>
    public string? SuggestedTestCommand { get; init; }

    /// <summary>Suggested test results format (VSTest or JUnit).</summary>
    public string? SuggestedTestResultsFormat { get; init; }

    /// <summary>Suggested coverage tool.</summary>
    public string? SuggestedCoverageTool { get; init; }

    /// <summary>Suggested coverage report path.</summary>
    public string? SuggestedCoverageReportPath { get; init; }

    /// <summary>Whether linting tools are available.</summary>
    public bool LintingAvailable { get; init; }

    /// <summary>Suggested lint command.</summary>
    public string? SuggestedLintCommand { get; init; }

    /// <summary>Whether Sonar configuration was detected.</summary>
    public bool SonarConfigDetected { get; init; }

    /// <summary>Suggested Sonar project key.</summary>
    public string? SuggestedSonarProjectKey { get; init; }

    /// <summary>Whether dependency scanning is available.</summary>
    public bool DependencyScanAvailable { get; init; }

    /// <summary>Suggested dependency scan tool.</summary>
    public string? SuggestedDependencyScanTool { get; init; }
}
