using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class AppPipelineCodeStepOptionTests
{
    private const string CiPipelinePath = ".azuredevops/pipelines/app-ci-code.pipeline.yml";
    private const string PrPipelinePath = ".azuredevops/pipelines/app-pr-code.pipeline.yml";
    private const string CiJobPath = ".azuredevops/jobs/app-ci-code.job.yml";
    private const string PrJobPath = ".azuredevops/jobs/app-pr-code.job.yml";
    private const string BuildCodeStepPath = ".azuredevops/steps/app-build-code.step.yml";

    private readonly AppPipelineGenerationEngine _sut = new(new IAppPipelineGenerator[]
    {
        new WebAppCodePipelineGenerator(),
    });

    [Fact]
    public void Given_SharedCodeTemplates_When_GenerateSharedTemplates_Then_TestAndCoverageOptionsFlowIntoBuildStep()
    {
        // Act
        var files = AppPipelineGenerationEngine.GenerateSharedTemplates();

        // Assert
        files[CiPipelinePath].Should()
            .Contain("- name: runUnitTests")
            .And.Contain("- name: publishTestResults")
            .And.Contain("- name: publishCodeCoverage")
            .And.Contain("runUnitTests: ${{ parameters.runUnitTests }}")
            .And.Contain("publishTestResults: ${{ parameters.publishTestResults }}")
            .And.Contain("publishCodeCoverage: ${{ parameters.publishCodeCoverage }}");

        files[PrPipelinePath].Should()
            .Contain("- name: runUnitTests")
            .And.Contain("- name: publishTestResults")
            .And.Contain("- name: publishCodeCoverage")
            .And.Contain("runUnitTests: ${{ parameters.runUnitTests }}")
            .And.Contain("publishTestResults: ${{ parameters.publishTestResults }}")
            .And.Contain("publishCodeCoverage: ${{ parameters.publishCodeCoverage }}");

        files[CiJobPath].Should()
            .Contain("- name: runUnitTests")
            .And.Contain("runUnitTests: ${{ parameters.runUnitTests }}")
            .And.Contain("publishTestResults: ${{ parameters.publishTestResults }}")
            .And.Contain("publishCodeCoverage: ${{ parameters.publishCodeCoverage }}");

        files[PrJobPath].Should()
            .Contain("- name: runUnitTests")
            .And.Contain("runUnitTests: ${{ parameters.runUnitTests }}")
            .And.Contain("publishTestResults: ${{ parameters.publishTestResults }}")
            .And.Contain("publishCodeCoverage: ${{ parameters.publishCodeCoverage }}");

        files[BuildCodeStepPath].Should()
            .Contain("- name: runUnitTests")
            .And.Contain("- name: publishTestResults")
            .And.Contain("- name: publishCodeCoverage")
            .And.Contain("${{ if and(eq(parameters.runUnitTests, true), eq(parameters.testCommand, '')) }}:")
            .And.Contain("${{ if and(eq(parameters.runUnitTests, true), eq(parameters.publishTestResults, true)) }}:")
            .And.Contain("${{ if and(eq(parameters.runUnitTests, true), eq(parameters.publishCodeCoverage, true)) }}:");
    }

    [Fact]
    public void Given_WebAppCodeWithUnitTestsEnabled_When_Generate_Then_CiAndPrWrappersForwardTestAndCoverageSwitches()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.WebAppCode();
        request.RunUnitTests = true;
        request.PublishCodeCoverage = true;

        // Act
        var result = _sut.Generate(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Files[AppPipelineFileNames.Ci].Should()
            .Contain("runUnitTests: true")
            .And.Contain("publishCodeCoverage: true")
            .And.NotContain("publishTestResults: true");

        result.Value.Files[AppPipelineFileNames.Pr].Should()
            .Contain("runUnitTests: true")
            .And.Contain("publishCodeCoverage: true")
            .And.NotContain("publishTestResults: true");
    }
}