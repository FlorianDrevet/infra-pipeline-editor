using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class AppPipelinePhase4StepOptionTests
{
    private const string DependencyScanStepPath = ".azuredevops/steps/app-dependency-scan.step.yml";
    private const string DependencyCacheStepPath = ".azuredevops/steps/app-dependency-cache.step.yml";
    private const string SmokeTestStepPath = ".azuredevops/steps/app-smoke-test.step.yml";
    private const string CiCodePipelinePath = ".azuredevops/pipelines/app-ci-code.pipeline.yml";
    private const string CiCodeJobPath = ".azuredevops/jobs/app-ci-code.job.yml";
    private const string ReleaseContainerJobPath = ".azuredevops/jobs/app-release-container.job.yml";
    private const string ReleaseCodeJobPath = ".azuredevops/jobs/app-release-code.job.yml";
    private const string ReleaseContainerPipelinePath = ".azuredevops/pipelines/app-release-container.pipeline.yml";
    private const string ReleaseCodePipelinePath = ".azuredevops/pipelines/app-release-code.pipeline.yml";

    private readonly AppPipelineGenerationEngine _sut = new(new IAppPipelineGenerator[]
    {
        new WebAppCodePipelineGenerator(),
    });

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ContainsDependencyScanStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files.Should().ContainKey(DependencyScanStepPath);
        files[DependencyScanStepPath].Should()
            .Contain("dependencyScanTool")
            .And.Contain("OWASPDependencyCheck")
            .And.Contain("NpmAudit")
            .And.Contain("PipAudit")
            .And.Contain("Publish dependency scan report");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ContainsDependencyCacheStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files.Should().ContainKey(DependencyCacheStepPath);
        files[DependencyCacheStepPath].Should()
            .Contain("Cache NuGet packages")
            .And.Contain("Cache npm packages")
            .And.Contain("Cache pip packages")
            .And.Contain("Cache Maven packages")
            .And.Contain("Cache@2");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ContainsSmokeTestStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files.Should().ContainKey(SmokeTestStepPath);
        files[SmokeTestStepPath].Should()
            .Contain("smokeTestCommand")
            .And.Contain("Run smoke tests");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_CiCodePipelineDeclaresPhase4Parameters()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[CiCodePipelinePath].Should()
            .Contain("- name: enableDependencyCache")
            .And.Contain("- name: runDependencyScan")
            .And.Contain("- name: dependencyScanTool")
            .And.Contain("enableDependencyCache: ${{ parameters.enableDependencyCache }}")
            .And.Contain("runDependencyScan: ${{ parameters.runDependencyScan }}")
            .And.Contain("dependencyScanTool: ${{ parameters.dependencyScanTool }}");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_CiCodeJobReferencesPhase4Steps()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[CiCodeJobPath].Should()
            .Contain("- name: enableDependencyCache")
            .And.Contain("- name: runDependencyScan")
            .And.Contain("- name: dependencyScanTool")
            .And.Contain("../steps/app-dependency-cache.step.yml")
            .And.Contain("../steps/app-dependency-scan.step.yml")
            .And.Contain("${{ if parameters.enableDependencyCache }}")
            .And.Contain("${{ if parameters.runDependencyScan }}");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ReleaseContainerPipelineDeclaresSmoke()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[ReleaseContainerPipelinePath].Should()
            .Contain("- name: runSmokeTests")
            .And.Contain("- name: smokeTestCommand")
            .And.Contain("runSmokeTests: ${{ parameters.runSmokeTests }}")
            .And.Contain("smokeTestCommand: ${{ parameters.smokeTestCommand }}");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ReleaseCodePipelineDeclaresSmoke()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[ReleaseCodePipelinePath].Should()
            .Contain("- name: runSmokeTests")
            .And.Contain("- name: smokeTestCommand")
            .And.Contain("runSmokeTests: ${{ parameters.runSmokeTests }}")
            .And.Contain("smokeTestCommand: ${{ parameters.smokeTestCommand }}");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ReleaseContainerJobReferencesSmokeStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[ReleaseContainerJobPath].Should()
            .Contain("- name: runSmokeTests")
            .And.Contain("- name: smokeTestCommand")
            .And.Contain("${{ if parameters.runSmokeTests }}")
            .And.Contain("../steps/app-smoke-test.step.yml");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ReleaseCodeJobReferencesSmokeStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[ReleaseCodeJobPath].Should()
            .Contain("- name: runSmokeTests")
            .And.Contain("- name: smokeTestCommand")
            .And.Contain("${{ if parameters.runSmokeTests }}")
            .And.Contain("../steps/app-smoke-test.step.yml");
    }

    [Fact]
    public void Given_WebAppCodeWithDependencyScan_When_Generate_Then_CiWrapperForwardsScanParameters()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.RunDependencyScan = true;
        request.DependencyScanTool = "NpmAudit";

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .Contain("runDependencyScan: true")
            .And.Contain("dependencyScanTool: 'NpmAudit'");
    }

    [Fact]
    public void Given_WebAppCodeWithDependencyCache_When_Generate_Then_CiWrapperForwardsCacheParameter()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.EnableDependencyCache = true;

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .Contain("enableDependencyCache: true");
    }

    [Fact]
    public void Given_WebAppCodeWithSmokeTests_When_Generate_Then_ReleaseWrapperForwardsSmokeParameters()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.RunSmokeTests = true;
        request.SmokeTestCommand = "curl -f https://myapp.azurewebsites.net/health";

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Release].Should()
            .Contain("runSmokeTests: true")
            .And.Contain("smokeTestCommand: 'curl -f https://myapp.azurewebsites.net/health'");
    }

    [Fact]
    public void Given_WebAppCodeWithoutPhase4Options_When_Generate_Then_WrappersOmitPhase4Parameters()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .NotContain("runDependencyScan: true")
            .And.NotContain("enableDependencyCache: true");

        result.Value.Files[AppPipelineFileNames.Release].Should()
            .NotContain("runSmokeTests: true");
    }

    [Fact]
    public void Given_DependencyScanStep_When_Rendered_Then_ContainsOwaspSection()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();
        var content = files[DependencyScanStepPath];

        content.Should()
            .Contain("${{ if eq(parameters.dependencyScanTool, 'OWASPDependencyCheck') }}")
            .And.Contain("OWASP Dependency-Check scan")
            .And.Contain("dependency-check.bat");
    }

    [Fact]
    public void Given_DependencyScanStep_When_Rendered_Then_ContainsNpmAuditSection()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();
        var content = files[DependencyScanStepPath];

        content.Should()
            .Contain("${{ if eq(parameters.dependencyScanTool, 'NpmAudit') }}")
            .And.Contain("npm audit --json")
            .And.Contain("npm-audit-report.json");
    }

    [Fact]
    public void Given_DependencyScanStep_When_Rendered_Then_ContainsPipAuditSection()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();
        var content = files[DependencyScanStepPath];

        content.Should()
            .Contain("${{ if eq(parameters.dependencyScanTool, 'PipAudit') }}")
            .And.Contain("pip-audit")
            .And.Contain("pip-audit-report.json");
    }

    [Fact]
    public void Given_DependencyCacheStep_When_Rendered_Then_ContainsStackSpecificCacheKeys()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();
        var content = files[DependencyCacheStepPath];

        content.Should()
            .Contain("**/packages.lock.json")
            .And.Contain("**/package-lock.json")
            .And.Contain("**/requirements.txt")
            .And.Contain("**/pom.xml");
    }
}