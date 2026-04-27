using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Tests.TestDoubles;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class AppPipelineGenerationEngineValidationTests
{
    [Fact]
    public void Given_InvalidDeploymentMode_When_Generate_Then_ThrowsArgumentExceptionWithKnownValuesInMessage()
    {
        // Arrange
        var sut = new AppPipelineGenerationEngine([]);
        var request = new AppPipelineGenerationRequest
        {
            ResourceName = "my-app",
            ConfigName = "config",
            ResourceType = AzureResourceTypes.WebApp,
            DeploymentMode = "Hybrid",
        };

        // Act
        var act = () => sut.Generate(request);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .Where(ex => ex.ParamName == nameof(request.DeploymentMode))
            .Where(ex => ex.Message.Contains("Hybrid"))
            .Where(ex => ex.Message.Contains(DeploymentModes.Code))
            .Where(ex => ex.Message.Contains(DeploymentModes.Container));
    }

    [Fact]
    public void Given_KnownDeploymentModeButNoMatchingGenerator_When_Generate_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new AppPipelineGenerationEngine([]);
        var request = new AppPipelineGenerationRequest
        {
            ResourceName = "my-app",
            ConfigName = "config",
            ResourceType = AzureResourceTypes.WebApp,
            DeploymentMode = DeploymentModes.Code,
        };

        // Act
        var act = () => sut.Generate(request);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*WebApp*Code*");
    }

    [Fact]
    public void Given_KnownDeploymentMode_When_Generate_Then_DispatchesToGenerator()
    {
        // Arrange
        var matchingGenerator = new StubAppPipelineGenerator(
            resourceType: AzureResourceTypes.WebApp,
            deploymentMode: DeploymentModes.Container);
        var unrelatedGenerator = new StubAppPipelineGenerator(
            resourceType: AzureResourceTypes.FunctionApp,
            deploymentMode: DeploymentModes.Code);
        var sut = new AppPipelineGenerationEngine([unrelatedGenerator, matchingGenerator]);
        var request = new AppPipelineGenerationRequest
        {
            ResourceName = "my-app",
            ConfigName = "config",
            ResourceType = AzureResourceTypes.WebApp,
            DeploymentMode = "container", // case-insensitive lookup
        };

        // Act
        var result = sut.Generate(request);

        // Assert
        matchingGenerator.InvocationCount.Should().Be(1);
        unrelatedGenerator.InvocationCount.Should().Be(0);
        matchingGenerator.LastRequest.Should().BeSameAs(request);
        result.Files.Should().ContainKey("stub.yml");
    }
}
