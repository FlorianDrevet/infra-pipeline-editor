using ErrorOr;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using InfraFlowSculptor.PipelineGeneration.Tests.TestDoubles;

namespace InfraFlowSculptor.PipelineGeneration.Tests;

public sealed class AppPipelineGenerationEngineValidationTests
{
    [Fact]
    public void Given_InvalidDeploymentMode_When_Generate_Then_ReturnsValidationErrorWithKnownValuesInMessage()
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
        var result = sut.Generate(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Generation.InvalidDeploymentMode");
        result.FirstError.Description.Should().Contain("Hybrid");
        result.FirstError.Description.Should().Contain(DeploymentModes.Code);
        result.FirstError.Description.Should().Contain(DeploymentModes.Container);
    }

    [Fact]
    public void Given_KnownDeploymentModeButNoMatchingGenerator_When_Generate_Then_ReturnsValidationError()
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
        var result = sut.Generate(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Generation.MissingAppPipelineGenerator");
        result.FirstError.Description.Should().Contain("WebApp");
        result.FirstError.Description.Should().Contain(DeploymentModes.Code);
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
        result.IsError.Should().BeFalse();
        matchingGenerator.InvocationCount.Should().Be(1);
        unrelatedGenerator.InvocationCount.Should().Be(0);
        matchingGenerator.LastRequest.Should().BeSameAs(request);
        result.Value.Files.Should().ContainKey("stub.yml");
    }
}
