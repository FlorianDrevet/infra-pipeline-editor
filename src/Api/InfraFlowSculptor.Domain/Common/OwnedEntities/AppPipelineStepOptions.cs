namespace InfraFlowSculptor.Domain.Common.OwnedEntities;

/// <summary>
/// Configurable pipeline step options for a compute resource's CI/CD pipeline.
/// Stored as an owned entity on WebApp, FunctionApp, and ContainerApp aggregates.
/// All boolean options default to <c>false</c> for backward compatibility.
/// </summary>
public sealed class AppPipelineStepOptions
{
    // ── Tests ──────────────────────────────────────────────────────────────

    /// <summary>Whether to run unit tests in the CI pipeline.</summary>
    public bool RunUnitTests { get; private set; }

    /// <summary>Custom test command. Auto-detected or user-specified.</summary>
    public string? TestCommand { get; private set; }

    /// <summary>Detected or user-specified test framework (xunit, nunit, jest, pytest, etc.).</summary>
    public string? TestFramework { get; private set; }

    /// <summary>Test results output format (VSTest or JUnit).</summary>
    public string? TestResultsFormat { get; private set; }

    /// <summary>Glob pattern for test result files.</summary>
    public string? TestResultsPath { get; private set; }

    /// <summary>Whether to publish test results to Azure DevOps.</summary>
    public bool PublishTestResults { get; private set; }

    // ── Coverage ───────────────────────────────────────────────────────────

    /// <summary>Whether to collect and publish code coverage.</summary>
    public bool PublishCodeCoverage { get; private set; }

    /// <summary>Coverage tool: Cobertura or JaCoCo.</summary>
    public string? CoverageTool { get; private set; }

    /// <summary>Path to the coverage report file.</summary>
    public string? CoverageReportPath { get; private set; }

    // ── Sonar ──────────────────────────────────────────────────────────────

    /// <summary>Whether to run SonarQube/SonarCloud analysis.</summary>
    public bool RunSonarAnalysis { get; private set; }

    /// <summary>Sonar project key.</summary>
    public string? SonarProjectKey { get; private set; }

    /// <summary>Sonar organization (SonarCloud only). Null means SonarQube Server.</summary>
    public string? SonarOrganization { get; private set; }

    /// <summary>Service connection name for Sonar in Azure DevOps.</summary>
    public string? SonarServiceConnection { get; private set; }

    // ── Linting ────────────────────────────────────────────────────────────

    /// <summary>Whether to run linting/formatting checks.</summary>
    public bool RunLinting { get; private set; }

    /// <summary>Custom lint command.</summary>
    public string? LintCommand { get; private set; }

    // ── Security ───────────────────────────────────────────────────────────

    /// <summary>Whether to scan dependencies for known vulnerabilities.</summary>
    public bool RunDependencyScan { get; private set; }

    /// <summary>Dependency scan tool identifier (OWASPDependencyCheck, NpmAudit, PipAudit, Snyk).</summary>
    public string? DependencyScanTool { get; private set; }

    // ── Build ──────────────────────────────────────────────────────────────

    /// <summary>Whether to run build validation on PR pipelines.</summary>
    public bool RunBuildValidation { get; private set; }

    // ── Cache ──────────────────────────────────────────────────────────────

    /// <summary>Whether to cache dependencies (NuGet, npm, pip) for faster builds.</summary>
    public bool EnableDependencyCache { get; private set; }

    // ── Post-deploy ────────────────────────────────────────────────────────

    /// <summary>Whether to run smoke tests after deployment.</summary>
    public bool RunSmokeTests { get; private set; }

    /// <summary>Custom smoke test command or URL to health-check.</summary>
    public string? SmokeTestCommand { get; private set; }

    /// <summary>Updates all pipeline step options at once.</summary>
    public void Update(AppPipelineStepOptionsData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        RunUnitTests = data.RunUnitTests;
        TestCommand = data.TestCommand;
        TestFramework = data.TestFramework;
        TestResultsFormat = data.TestResultsFormat;
        TestResultsPath = data.TestResultsPath;
        PublishTestResults = data.PublishTestResults;
        PublishCodeCoverage = data.PublishCodeCoverage;
        CoverageTool = data.CoverageTool;
        CoverageReportPath = data.CoverageReportPath;
        RunSonarAnalysis = data.RunSonarAnalysis;
        SonarProjectKey = data.SonarProjectKey;
        SonarOrganization = data.SonarOrganization;
        SonarServiceConnection = data.SonarServiceConnection;
        RunLinting = data.RunLinting;
        LintCommand = data.LintCommand;
        RunDependencyScan = data.RunDependencyScan;
        DependencyScanTool = data.DependencyScanTool;
        RunBuildValidation = data.RunBuildValidation;
        EnableDependencyCache = data.EnableDependencyCache;
        RunSmokeTests = data.RunSmokeTests;
        SmokeTestCommand = data.SmokeTestCommand;
    }
}
