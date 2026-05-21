using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class AppPipelinePhase5SonarLintTests
{
    private const string SonarStepPath = ".azuredevops/steps/app-sonar-analysis.step.yml";
    private const string LintStepPath = ".azuredevops/steps/app-linting.step.yml";
    private const string CiCodePipelinePath = ".azuredevops/pipelines/app-ci-code.pipeline.yml";
    private const string CiCodeJobPath = ".azuredevops/jobs/app-ci-code.job.yml";

    private readonly AppPipelineGenerationEngine _sut = new(new IAppPipelineGenerator[]
    {
        new WebAppCodePipelineGenerator(),
    });

    // ── Step template registration ──────────────────────────────────────────

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ContainsSonarAnalysisStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files.Should().ContainKey(SonarStepPath);
        files[SonarStepPath].Should()
            .Contain("sonarProjectKey")
            .And.Contain("sonarOrganization")
            .And.Contain("sonarServiceConnection")
            .And.Contain("SonarQubeAnalyze@6");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_ContainsLintingStep()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files.Should().ContainKey(LintStepPath);
        files[LintStepPath].Should()
            .Contain("lintCommand")
            .And.Contain("Run linting");
    }

    // ── CI code pipeline parameter forwarding ───────────────────────────────

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_CiCodePipelineDeclaresPhase5Parameters()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[CiCodePipelinePath].Should()
            .Contain("- name: runSonarAnalysis")
            .And.Contain("- name: sonarProjectKey")
            .And.Contain("- name: sonarOrganization")
            .And.Contain("- name: sonarServiceConnection")
            .And.Contain("- name: runLinting")
            .And.Contain("- name: lintCommand")
            .And.Contain("runSonarAnalysis: ${{ parameters.runSonarAnalysis }}")
            .And.Contain("runLinting: ${{ parameters.runLinting }}")
            .And.Contain("lintCommand: ${{ parameters.lintCommand }}");
    }

    // ── CI code job parameter declarations and step references ───────────────

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_CiCodeJobDeclaresPhase5Parameters()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[CiCodeJobPath].Should()
            .Contain("- name: runSonarAnalysis")
            .And.Contain("- name: sonarProjectKey")
            .And.Contain("- name: sonarOrganization")
            .And.Contain("- name: sonarServiceConnection")
            .And.Contain("- name: runLinting")
            .And.Contain("- name: lintCommand");
    }

    [Fact]
    public void Given_SharedTemplates_When_GenerateAll_Then_CiCodeJobReferencesPhase5Steps()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        files[CiCodeJobPath].Should()
            .Contain("${{ if parameters.runSonarAnalysis }}")
            .And.Contain("../steps/app-sonar-analysis.step.yml")
            .And.Contain("${{ if parameters.runLinting }}")
            .And.Contain("../steps/app-linting.step.yml");
    }

    // ── CI wrapper forwarding ───────────────────────────────────────────────

    [Fact]
    public void Given_WebAppCodeWithSonar_When_Generate_Then_CiWrapperForwardsSonarParameters()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.RunSonarAnalysis = true;
        request.SonarProjectKey = "my-project";
        request.SonarOrganization = "my-org";
        request.SonarServiceConnection = "SonarCloud";

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .Contain("runSonarAnalysis: true")
            .And.Contain("sonarProjectKey: 'my-project'")
            .And.Contain("sonarOrganization: 'my-org'")
            .And.Contain("sonarServiceConnection: 'SonarCloud'");
    }

    [Fact]
    public void Given_WebAppCodeWithLinting_When_Generate_Then_CiWrapperForwardsLintParameters()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.RunLinting = true;
        request.LintCommand = "dotnet format --verify-no-changes";

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .Contain("runLinting: true")
            .And.Contain("lintCommand: 'dotnet format --verify-no-changes'");
    }

    [Fact]
    public void Given_WebAppCodeWithoutPhase5_When_Generate_Then_OmitsSonarAndLintParameters()
    {
        var request = AppPipelineRequestFixtures.WebAppCode();

        var result = _sut.Generate(request);

        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .NotContain("runSonarAnalysis: true")
            .And.NotContain("runLinting: true");
    }

    // ── Step content assertions ─────────────────────────────────────────────

    [Fact]
    public void Given_SonarStep_When_Rendered_Then_ContainsPrepareAnalyzePublish()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();
        var content = files[SonarStepPath];

        content.Should()
            .Contain("SonarQubePrepare@6")
            .And.Contain("SonarQubeAnalyze@6")
            .And.Contain("SonarQubePublish@6");
    }

    [Fact]
    public void Given_LintStep_When_Rendered_Then_ContainsPowerShellExecution()
    {
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();
        var content = files[LintStepPath];

        content.Should()
            .Contain("powershell:")
            .And.Contain("parameters.lintCommand");
    }
}
