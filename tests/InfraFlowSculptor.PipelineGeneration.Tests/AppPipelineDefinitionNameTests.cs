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
}