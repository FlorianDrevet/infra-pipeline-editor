using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore.Models;
using NSubstitute;

namespace InfraFlowSculptor.BicepGeneration.Tests;

public sealed class BicepGenerationEngineTests
{
    [Fact]
    public void Given_ResourceWithoutRegisteredGenerator_When_Generate_Then_ReturnsValidationError()
    {
        // Arrange
        var sut = new BicepGenerationEngine(new BicepGenerationPipeline([new ModuleBuildStage([])]));
        var request = new GenerationRequest
        {
            Resources =
            [
                new ResourceDefinition
                {
                    ResourceId = Guid.NewGuid(),
                    Name = "my-unknown",
                    Type = "Microsoft.Unknown/resources",
                },
            ],
        };

        // Act
        var result = sut.Generate(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Generation.UnsupportedBicepResourceType");
        result.FirstError.Description.Should().Contain("my-unknown");
        result.FirstError.Description.Should().Contain("Microsoft.Unknown/resources");
    }

    [Fact]
    public void Given_InvalidKeyVaultSecretNameFailure_When_Generate_Then_ReturnsValidationError()
    {
        // Arrange
        const string errorMessage = "Key Vault secret name 'JWT_SECRET' for resource 'api' is invalid. Secret names must be alphanumeric or hyphenated.";

        var stage = Substitute.For<IBicepGenerationStage>();
        stage.Order.Returns(100);
        stage.When(current => current.Execute(Arg.Any<BicepGenerationContext>()))
            .Do(_ => throw new InvalidOperationException(errorMessage));

        var sut = new BicepGenerationEngine(new BicepGenerationPipeline([stage]));

        // Act
        var result = sut.Generate(new GenerationRequest());

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Generation.InvalidBicepConfiguration");
        result.FirstError.Description.Should().Be(errorMessage);
    }

    [Fact]
    public void Given_CancelledToken_When_Generate_Then_ThrowsOperationCanceledException()
    {
        // Arrange
        var stage = Substitute.For<IBicepGenerationStage>();
        stage.Order.Returns(100);

        var sut = new BicepGenerationEngine(new BicepGenerationPipeline([stage]));
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var act = () => sut.Generate(new GenerationRequest(), cancellationTokenSource.Token);

        // Assert
        act.Should().Throw<OperationCanceledException>();
        stage.DidNotReceive().Execute(Arg.Any<BicepGenerationContext>());
    }
}