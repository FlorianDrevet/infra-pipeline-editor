using FluentAssertions;
using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Bootstrap;

public sealed class BootstrapPipelineGenerationEngineTests
{
    [Fact]
    public void Given_PipelineDefinitions_When_Generate_Then_CreateStepTemporarilyDisablesNativeStderrFailures()
    {
        // Arrange
        var sut = new BootstrapPipelineGenerationEngine();
        var request = new BootstrapGenerationRequest
        {
            OrganizationName = "contoso",
            ProjectName = "ifs",
            RepositoryName = "ifs",
            DefaultBranch = "main",
            Pipelines =
            [
                new BootstrapPipelineDefinition(
                    Name: "Core - CI",
                    YamlPath: ".azuredevops/core/ci.pipeline.yml",
                    Folder: "\\Core")
            ],
        };

        // Act
        var result = sut.Generate(request);

        // Assert
        var content = result.TemplateFiles["bootstrap.pipeline.yml"];
        content.Should().Contain("$previousErrorActionPreference = $ErrorActionPreference");
        content.Should().Contain("$ErrorActionPreference = 'Continue'");
        content.Should().Contain("--only-show-errors");
        content.Should().Contain("$createExitCode = $LASTEXITCODE");
        content.Should().Contain("$ErrorActionPreference = $previousErrorActionPreference");
        content.Should().NotContain("$createOutput = az pipelines create --name 'Core - CI'");
    }
}