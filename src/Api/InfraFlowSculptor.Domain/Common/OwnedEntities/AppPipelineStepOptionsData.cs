namespace InfraFlowSculptor.Domain.Common.OwnedEntities;

/// <summary>
/// Groups all values needed to update <see cref="AppPipelineStepOptions"/> without passing a long flat parameter list.
/// </summary>
public sealed class AppPipelineStepOptionsData
{
    /// <summary>Gets or sets whether unit tests run in the CI pipeline.</summary>
    public bool RunUnitTests { get; init; }

    /// <summary>Gets or sets the custom test command.</summary>
    public string? TestCommand { get; init; }

    /// <summary>Gets or sets the detected or user-specified test framework.</summary>
    public string? TestFramework { get; init; }

    /// <summary>Gets or sets the test results output format.</summary>
    public string? TestResultsFormat { get; init; }

    /// <summary>Gets or sets the glob pattern for test result files.</summary>
    public string? TestResultsPath { get; init; }

    /// <summary>Gets or sets whether test results are published to Azure DevOps.</summary>
    public bool PublishTestResults { get; init; }

    /// <summary>Gets or sets whether code coverage is collected and published.</summary>
    public bool PublishCodeCoverage { get; init; }

    /// <summary>Gets or sets the coverage tool identifier.</summary>
    public string? CoverageTool { get; init; }

    /// <summary>Gets or sets the coverage report path.</summary>
    public string? CoverageReportPath { get; init; }

    /// <summary>Gets or sets whether Sonar analysis runs.</summary>
    public bool RunSonarAnalysis { get; init; }

    /// <summary>Gets or sets the Sonar project key.</summary>
    public string? SonarProjectKey { get; init; }

    /// <summary>Gets or sets the Sonar organization.</summary>
    public string? SonarOrganization { get; init; }

    /// <summary>Gets or sets the Sonar service connection name.</summary>
    public string? SonarServiceConnection { get; init; }

    /// <summary>Gets or sets whether linting runs.</summary>
    public bool RunLinting { get; init; }

    /// <summary>Gets or sets the lint command.</summary>
    public string? LintCommand { get; init; }

    /// <summary>Gets or sets whether dependency scanning runs.</summary>
    public bool RunDependencyScan { get; init; }

    /// <summary>Gets or sets the dependency scan tool identifier.</summary>
    public string? DependencyScanTool { get; init; }

    /// <summary>Gets or sets whether PR build validation runs.</summary>
    public bool RunBuildValidation { get; init; }

    /// <summary>Gets or sets whether dependency caching is enabled.</summary>
    public bool EnableDependencyCache { get; init; }

    /// <summary>Gets or sets whether smoke tests run after deployment.</summary>
    public bool RunSmokeTests { get; init; }

    /// <summary>Gets or sets the smoke-test command or health-check URL.</summary>
    public string? SmokeTestCommand { get; init; }
}