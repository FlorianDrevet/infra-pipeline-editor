using InfraFlowSculptor.PipelineGeneration.Generators;
using InfraFlowSculptor.PipelineGeneration.Generators.App;
using InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

/// <summary>
/// Verifies that application release pipelines reference the prefixed CI pipeline definition names.
/// </summary>
public sealed class AppPipelineDefinitionNameTests
{
    private readonly AppPipelineGenerationEngine _sut = new(new IAppPipelineGenerator[]
    {
        new ContainerAppPipelineGenerator(),
        new WebAppCodePipelineGenerator(),
        new WebAppContainerPipelineGenerator(),
        new FunctionAppCodePipelineGenerator(),
        new FunctionAppContainerPipelineGenerator(),
    });

    [Fact]
    public void Given_WebAppCodeRequest_When_Generate_Then_ReleasePipelineUsesCodePrefixedCiSource()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.WebAppCode();

        // Act
        var result = _sut.Generate(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Files["release.app-pipeline.yml"].Should().Contain("# Expected CI pipeline definition name: [Code] core - myweb - CI");
        result.Value.Files["release.app-pipeline.yml"].Should().Contain("source: '[Code] core - myweb - CI'");
    }

    [Fact]
    public void Given_WebAppCodeRequest_When_Generate_Then_PrPipelineUsesCodePrefixedPrName()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.WebAppCode();

        // Act
        var result = _sut.Generate(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Files.Should().ContainKey("pr.app-pipeline.yml");
        result.Value.Files["pr.app-pipeline.yml"].Should().Contain("# Expected PR pipeline definition name: [Code] core - myweb - PR");
        result.Value.Files["pr.app-pipeline.yml"].Should().Contain("pr:");
        result.Value.Files["pr.app-pipeline.yml"].Should().Contain("template: ../../../Common/pipelines/app-pr-code.pipeline.yml");
    }

    [Fact]
    public void Given_ContainerAppRequest_When_Generate_Then_PrPipelineValidatesWithoutRegistryConnection()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.ContainerApp();

        // Act
        var result = _sut.Generate(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Files.Should().ContainKey("pr.app-pipeline.yml");
        result.Value.Files["pr.app-pipeline.yml"].Should().Contain("template: ../../../Common/pipelines/app-pr-container.pipeline.yml");
        result.Value.Files["pr.app-pipeline.yml"].Should().NotContain("containerRegistryServiceConnection");
        result.Value.Files["pr.app-pipeline.yml"].Should().NotContain("app-acr-login");
    }

    [Fact]
    public void Given_CustomSourceCodePath_When_Generate_Then_CiAndPrTriggersWatchTheSourceFolder()
    {
        // Arrange
        var request = AppPipelineRequestFixtures.ContainerApp();
        request.SourceCodePath = "src/Api";

        // Act
        var result = _sut.Generate(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Files["ci.app-pipeline.yml"].Should().Contain("- src/Api/*");
        result.Value.Files["pr.app-pipeline.yml"].Should().Contain("- src/Api/*");
    }
}