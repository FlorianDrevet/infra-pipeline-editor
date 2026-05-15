using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Common.Requests;

/// <summary>
/// DTO for configuring pipeline step options on a compute resource.
/// All boolean options default to <c>false</c> for backward compatibility.
/// </summary>
public sealed class PipelineStepOptionsDto
{
    // ── Tests ──────────────────────────────────────────────────────────────

    /// <summary>Whether to run unit tests in the CI pipeline.</summary>
    public bool RunUnitTests { get; init; }

    /// <summary>Custom test command. Auto-detected or user-specified.</summary>
    [MaxLength(500)]
    public string? TestCommand { get; init; }

    /// <summary>Detected or user-specified test framework (xunit, nunit, jest, pytest, etc.).</summary>
    [MaxLength(50)]
    public string? TestFramework { get; init; }

    /// <summary>Test results output format (VSTest or JUnit).</summary>
    [MaxLength(20)]
    public string? TestResultsFormat { get; init; }

    /// <summary>Glob pattern for test result files.</summary>
    [MaxLength(500)]
    public string? TestResultsPath { get; init; }

    /// <summary>Whether to publish test results to Azure DevOps.</summary>
    public bool PublishTestResults { get; init; }

    // ── Coverage ───────────────────────────────────────────────────────────

    /// <summary>Whether to collect and publish code coverage.</summary>
    public bool PublishCodeCoverage { get; init; }

    /// <summary>Coverage tool: Cobertura or JaCoCo.</summary>
    [MaxLength(20)]
    public string? CoverageTool { get; init; }

    /// <summary>Path to the coverage report file.</summary>
    [MaxLength(500)]
    public string? CoverageReportPath { get; init; }

    // ── Sonar ──────────────────────────────────────────────────────────────

    /// <summary>Whether to run SonarQube/SonarCloud analysis.</summary>
    public bool RunSonarAnalysis { get; init; }

    /// <summary>Sonar project key.</summary>
    [MaxLength(200)]
    public string? SonarProjectKey { get; init; }

    /// <summary>Sonar organization (SonarCloud only). Null means SonarQube Server.</summary>
    [MaxLength(200)]
    public string? SonarOrganization { get; init; }

    /// <summary>Service connection name for Sonar in Azure DevOps.</summary>
    [MaxLength(200)]
    public string? SonarServiceConnection { get; init; }

    // ── Linting ────────────────────────────────────────────────────────────

    /// <summary>Whether to run linting/formatting checks.</summary>
    public bool RunLinting { get; init; }

    /// <summary>Custom lint command.</summary>
    [MaxLength(500)]
    public string? LintCommand { get; init; }

    // ── Security ───────────────────────────────────────────────────────────

    /// <summary>Whether to scan dependencies for known vulnerabilities.</summary>
    public bool RunDependencyScan { get; init; }

    /// <summary>Dependency scan tool identifier (OWASPDependencyCheck, NpmAudit, PipAudit, Snyk).</summary>
    [MaxLength(50)]
    public string? DependencyScanTool { get; init; }

    // ── Build ──────────────────────────────────────────────────────────────

    /// <summary>Whether to run build validation on PR pipelines.</summary>
    public bool RunBuildValidation { get; init; }

    // ── Cache ──────────────────────────────────────────────────────────────

    /// <summary>Whether to cache dependencies (NuGet, npm, pip) for faster builds.</summary>
    public bool EnableDependencyCache { get; init; }

    // ── Post-deploy ────────────────────────────────────────────────────────

    /// <summary>Whether to run smoke tests after deployment.</summary>
    public bool RunSmokeTests { get; init; }

    /// <summary>Custom smoke test command or URL to health-check.</summary>
    [MaxLength(500)]
    public string? SmokeTestCommand { get; init; }
}
