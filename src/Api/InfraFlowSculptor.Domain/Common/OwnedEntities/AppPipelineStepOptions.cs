using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities;

/// <summary>
/// Configurable pipeline step options for a compute resource's CI/CD pipeline.
/// Stored as an owned entity on WebApp, FunctionApp, and ContainerApp aggregates.
/// All boolean options default to <c>false</c> for backward compatibility.
/// </summary>
public sealed class AppPipelineStepOptions
{
    private const string ProfileStackMismatchMessage = "Pipeline stack profile must match the selected application stack.";
    private const string DefaultNodeTestScriptName = "test";
    private const string DefaultNodeLintScriptName = "lint";

    private AppPipelineStackProfile? _profile;

    /// <summary>Application stack used to drive application pipeline behavior.</summary>
    public ApplicationStack Stack { get; private set; } = ApplicationStack.Unknown;

    /// <summary>Typed stack-specific pipeline profile for the selected application stack.</summary>
    public AppPipelineStackProfile? Profile => _profile ?? BuildProfileFromSnapshot();

    private ApplicationStack? ProfileStack { get; set; }

    private DotNetTestFramework? DotNetProfileTestFramework { get; set; }

    private bool? DotNetProfileCollectCoverage { get; set; }

    private string? DotNetProfileCustomTestProjectGlob { get; set; }

    private NodePackageManager? NodeJsProfilePackageManager { get; set; }

    private NodeTestFramework? NodeJsProfileTestFramework { get; set; }

    private bool? NodeJsProfileRunLintScript { get; set; }

    private string? NodeJsProfileTestScriptName { get; set; }

    private string? NodeJsProfileLintScriptName { get; set; }

    private NodePackageManager? AngularProfilePackageManager { get; set; }

    private bool? AngularProfileRunNgTest { get; set; }

    private bool? AngularProfileRunNgLint { get; set; }

    private bool? AngularProfileRunNgBuildProduction { get; set; }

    private string? AngularProfileProjectName { get; set; }

    private JavaBuildTool? JavaProfileBuildTool { get; set; }

    private JavaTestFramework? JavaProfileTestFramework { get; set; }

    private bool? JavaProfileCollectCoverage { get; set; }

    private PythonPackageManager? PythonProfilePackageManager { get; set; }

    private PythonTestFramework? PythonProfileTestFramework { get; set; }

    private bool? PythonProfileCollectCoverage { get; set; }

    private string? StaticSiteProfileBuildCommand { get; set; }

    private string? StaticSiteProfileOutputDirectory { get; set; }

    private string? CustomProfileTestCommand { get; set; }

    private string? CustomProfileLintCommand { get; set; }

    private string? CustomProfileBuildCommand { get; set; }

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

        var stack = data.Stack ?? ApplicationStack.Unknown;

        if (data.Profile is not null && data.Profile.Stack.Value != stack.Value)
        {
            throw new ArgumentException(ProfileStackMismatchMessage, nameof(data));
        }

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

        Stack = stack;
        _profile = data.Profile;
        ApplyProfileSnapshot(data.Profile);
    }

    private AppPipelineStackProfile? BuildProfileFromSnapshot()
    {
        return ProfileStack?.Value switch
        {
            ApplicationStack.ApplicationStackEnum.DotNet => DotNetPipelineProfile.Create(
                DotNetProfileTestFramework ?? DotNetTestFramework.XUnit,
                DotNetProfileCollectCoverage ?? false,
                DotNetProfileCustomTestProjectGlob),
            ApplicationStack.ApplicationStackEnum.NodeJs => NodeJsPipelineProfile.Create(
                NodeJsProfilePackageManager ?? NodePackageManager.Npm,
                NodeJsProfileTestFramework ?? NodeTestFramework.Jest,
                NodeJsProfileRunLintScript ?? false,
                NodeJsProfileTestScriptName ?? DefaultNodeTestScriptName,
                NodeJsProfileLintScriptName ?? DefaultNodeLintScriptName),
            ApplicationStack.ApplicationStackEnum.Angular => AngularPipelineProfile.Create(
                AngularProfilePackageManager ?? NodePackageManager.Npm,
                AngularProfileRunNgTest ?? true,
                AngularProfileRunNgLint ?? false,
                AngularProfileRunNgBuildProduction ?? true,
                AngularProfileProjectName),
            ApplicationStack.ApplicationStackEnum.Java => JavaPipelineProfile.Create(
                JavaProfileBuildTool ?? JavaBuildTool.Maven,
                JavaProfileTestFramework ?? JavaTestFramework.JUnit5,
                JavaProfileCollectCoverage ?? false),
            ApplicationStack.ApplicationStackEnum.Python => PythonPipelineProfile.Create(
                PythonProfilePackageManager ?? PythonPackageManager.Pip,
                PythonProfileTestFramework ?? PythonTestFramework.Pytest,
                PythonProfileCollectCoverage ?? false),
            ApplicationStack.ApplicationStackEnum.StaticSite when HasStaticSiteProfileSnapshot() =>
                StaticSitePipelineProfile.Create(StaticSiteProfileBuildCommand!, StaticSiteProfileOutputDirectory!),
            ApplicationStack.ApplicationStackEnum.Custom => CustomPipelineProfile.Create(
                CustomProfileTestCommand,
                CustomProfileLintCommand,
                CustomProfileBuildCommand),
            _ => null,
        };
    }

    private void ApplyProfileSnapshot(AppPipelineStackProfile? profile)
    {
        ClearProfileSnapshot();

        if (profile is null)
        {
            return;
        }

        ProfileStack = profile.Stack;

        switch (profile)
        {
            case DotNetPipelineProfile dotNetProfile:
                DotNetProfileTestFramework = dotNetProfile.TestFramework;
                DotNetProfileCollectCoverage = dotNetProfile.CollectCoverage;
                DotNetProfileCustomTestProjectGlob = dotNetProfile.CustomTestProjectGlob;
                break;
            case NodeJsPipelineProfile nodeJsProfile:
                NodeJsProfilePackageManager = nodeJsProfile.PackageManager;
                NodeJsProfileTestFramework = nodeJsProfile.TestFramework;
                NodeJsProfileRunLintScript = nodeJsProfile.RunLintScript;
                NodeJsProfileTestScriptName = nodeJsProfile.TestScriptName;
                NodeJsProfileLintScriptName = nodeJsProfile.LintScriptName;
                break;
            case AngularPipelineProfile angularProfile:
                AngularProfilePackageManager = angularProfile.PackageManager;
                AngularProfileRunNgTest = angularProfile.RunNgTest;
                AngularProfileRunNgLint = angularProfile.RunNgLint;
                AngularProfileRunNgBuildProduction = angularProfile.RunNgBuildProduction;
                AngularProfileProjectName = angularProfile.ProjectName;
                break;
            case JavaPipelineProfile javaProfile:
                JavaProfileBuildTool = javaProfile.BuildTool;
                JavaProfileTestFramework = javaProfile.TestFramework;
                JavaProfileCollectCoverage = javaProfile.CollectCoverage;
                break;
            case PythonPipelineProfile pythonProfile:
                PythonProfilePackageManager = pythonProfile.PackageManager;
                PythonProfileTestFramework = pythonProfile.TestFramework;
                PythonProfileCollectCoverage = pythonProfile.CollectCoverage;
                break;
            case StaticSitePipelineProfile staticSiteProfile:
                StaticSiteProfileBuildCommand = staticSiteProfile.BuildCommand;
                StaticSiteProfileOutputDirectory = staticSiteProfile.OutputDirectory;
                break;
            case CustomPipelineProfile customProfile:
                CustomProfileTestCommand = customProfile.CustomTestCommand;
                CustomProfileLintCommand = customProfile.CustomLintCommand;
                CustomProfileBuildCommand = customProfile.CustomBuildCommand;
                break;
        }
    }

    private void ClearProfileSnapshot()
    {
        ProfileStack = null;
        DotNetProfileTestFramework = null;
        DotNetProfileCollectCoverage = null;
        DotNetProfileCustomTestProjectGlob = null;
        NodeJsProfilePackageManager = null;
        NodeJsProfileTestFramework = null;
        NodeJsProfileRunLintScript = null;
        NodeJsProfileTestScriptName = null;
        NodeJsProfileLintScriptName = null;
        AngularProfilePackageManager = null;
        AngularProfileRunNgTest = null;
        AngularProfileRunNgLint = null;
        AngularProfileRunNgBuildProduction = null;
        AngularProfileProjectName = null;
        JavaProfileBuildTool = null;
        JavaProfileTestFramework = null;
        JavaProfileCollectCoverage = null;
        PythonProfilePackageManager = null;
        PythonProfileTestFramework = null;
        PythonProfileCollectCoverage = null;
        StaticSiteProfileBuildCommand = null;
        StaticSiteProfileOutputDirectory = null;
        CustomProfileTestCommand = null;
        CustomProfileLintCommand = null;
        CustomProfileBuildCommand = null;
    }

    private bool HasStaticSiteProfileSnapshot()
    {
        return !string.IsNullOrWhiteSpace(StaticSiteProfileBuildCommand)
            && !string.IsNullOrWhiteSpace(StaticSiteProfileOutputDirectory);
    }
}
