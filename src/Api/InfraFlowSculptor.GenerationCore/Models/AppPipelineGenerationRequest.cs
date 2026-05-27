namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Request model for application pipeline generation,
/// containing resource-specific CI/CD configuration data.
/// </summary>
public class AppPipelineGenerationRequest
{
    /// <summary>Display name of the target Azure resource.</summary>
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>User-friendly application name for pipeline display. Falls back to <see cref="ResourceName"/> if null.</summary>
    public string? ApplicationName { get; set; }

    /// <summary>Azure resource type identifier (e.g., AzureResourceTypes.WebApp).</summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>Deployment mode: "Code" or "Container".</summary>
    public string DeploymentMode { get; set; } = string.Empty;

    /// <summary>Application stack identifier (DotNet, NodeJs, Angular, Java, Python, StaticSite, Custom). Null or empty means Unknown.</summary>
    public string? ApplicationStack { get; set; }

    /// <summary>Relative path to the Dockerfile in the repository (container mode).</summary>
    public string? DockerfilePath { get; set; }

    /// <summary>Relative path to the source code folder (code mode).</summary>
    public string? SourceCodePath { get; set; }

    /// <summary>Optional custom build command (e.g., "dotnet publish -c Release").</summary>
    public string? BuildCommand { get; set; }

    /// <summary>Base Docker image name without tag (e.g., "myapp/api").</summary>
    public string? DockerImageName { get; set; }

    /// <summary>Name of the ACR resource (e.g., "myregistry").</summary>
    public string? ContainerRegistryName { get; set; }

    /// <summary>Environment-scoped Azure DevOps Docker/ACR service connections used by container CI pipelines.</summary>
    public IReadOnlyList<ContainerRegistryServiceConnectionDefinition> ContainerRegistryServiceConnections { get; set; } = [];

    /// <summary>
    /// Immutable tag pattern used by CI metadata generation.
    /// Supported tokens are <c>{buildNumber}</c>, <c>{shortSha}</c>, and <c>{branch}</c>.
    /// </summary>
    public string? ImageTagPattern { get; set; } = "{buildNumber}-{shortSha}";

    /// <summary>
    /// Strategy used to promote container images between environments.
    /// </summary>
    public AppPipelinePromotionStrategy PromotionStrategy { get; set; } = AppPipelinePromotionStrategy.AcrImport;

    /// <summary>Authentication mode used for Azure Container Registry access.</summary>
    public string? AcrAuthMode { get; set; }

    /// <summary>Runtime stack identifier (e.g., "DOTNETCORE", "NODE").</summary>
    public string? RuntimeStack { get; set; }

    /// <summary>Runtime version (e.g., "8.0", "20").</summary>
    public string? RuntimeVersion { get; set; }

    /// <summary>Optional explicit test command executed before packaging code-based deployments.</summary>
    public string? TestCommand { get; set; }

    /// <summary>Indicates whether Trivy and SBOM generation steps are emitted for container CI pipelines.</summary>
    public bool EnableSecurityScans { get; set; } = true;

    // ── Pipeline Step Options ──────────────────────────────────────────────────

    /// <summary>Whether to run unit tests in the CI pipeline.</summary>
    public bool RunUnitTests { get; set; }

    /// <summary>Test framework identifier (xunit, nunit, jest, pytest, etc.).</summary>
    public string? TestFramework { get; set; }

    /// <summary>Test results output format (VSTest or JUnit).</summary>
    public string? TestResultsFormat { get; set; }

    /// <summary>Glob pattern for test result files.</summary>
    public string? TestResultsPath { get; set; }

    /// <summary>Whether to publish test results to Azure DevOps.</summary>
    public bool PublishTestResults { get; set; }

    /// <summary>Whether to collect and publish code coverage.</summary>
    public bool PublishCodeCoverage { get; set; }

    /// <summary>Coverage tool: Cobertura or JaCoCo.</summary>
    public string? CoverageTool { get; set; }

    /// <summary>Path to the coverage report file.</summary>
    public string? CoverageReportPath { get; set; }

    /// <summary>Whether to run SonarQube/SonarCloud analysis.</summary>
    public bool RunSonarAnalysis { get; set; }

    /// <summary>Sonar project key.</summary>
    public string? SonarProjectKey { get; set; }

    /// <summary>Sonar organization (SonarCloud only).</summary>
    public string? SonarOrganization { get; set; }

    /// <summary>Service connection name for Sonar in Azure DevOps.</summary>
    public string? SonarServiceConnection { get; set; }

    /// <summary>Whether to run linting/formatting checks.</summary>
    public bool RunLinting { get; set; }

    /// <summary>Custom lint command.</summary>
    public string? LintCommand { get; set; }

    /// <summary>Whether to scan dependencies for known vulnerabilities.</summary>
    public bool RunDependencyScan { get; set; }

    /// <summary>Dependency scan tool identifier (OWASPDependencyCheck, NpmAudit, PipAudit, Snyk).</summary>
    public string? DependencyScanTool { get; set; }

    /// <summary>Whether to run build validation on PR pipelines.</summary>
    public bool RunBuildValidation { get; set; }

    /// <summary>Whether to cache dependencies for faster builds.</summary>
    public bool EnableDependencyCache { get; set; }

    /// <summary>Whether to run smoke tests after deployment.</summary>
    public bool RunSmokeTests { get; set; }

    /// <summary>Custom smoke test command or URL to health-check.</summary>
    public string? SmokeTestCommand { get; set; }

    /// <summary>Whether to run license compliance checks on dependencies.</summary>
    public bool RunLicenseCheck { get; set; }

    /// <summary>License check tool identifier (license-checker, dotnet-delice, licensefinder).</summary>
    public string? LicenseCheckTool { get; set; }

    /// <summary>Whether to send a webhook notification at the end of the pipeline.</summary>
    public bool EnableNotifications { get; set; }

    /// <summary>Webhook URL for Teams/Slack notification.</summary>
    public string? NotificationWebhookUrl { get; set; }

    /// <summary>Environment definitions with service connections and subscription IDs.</summary>
    public IReadOnlyList<EnvironmentDefinition> Environments { get; set; } = [];

    /// <summary>Pipeline variable groups to reference in the generated pipeline.</summary>
    public IReadOnlyList<PipelineVariableGroupDefinition> PipelineVariableGroups { get; set; } = [];

    /// <summary>Name of the infrastructure configuration this resource belongs to.</summary>
    public string ConfigName { get; set; } = string.Empty;

    /// <summary>Whether the project uses mono-repo pipeline structure.</summary>
    public bool IsMonoRepo { get; set; }

    /// <summary>
    /// Self-hosted agent pool name. When set, pipelines use <c>pool: name: 'value'</c>.
    /// When <c>null</c>, pipelines use the Microsoft-hosted pool (<c>vmImage: ubuntu-latest</c>).
    /// </summary>
    public string? AgentPoolName { get; set; }
}
