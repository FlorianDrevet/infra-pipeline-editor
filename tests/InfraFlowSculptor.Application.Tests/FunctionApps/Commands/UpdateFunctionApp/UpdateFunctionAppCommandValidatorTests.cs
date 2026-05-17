using FluentAssertions;
using InfraFlowSculptor.Application.FunctionApps.Commands.UpdateFunctionApp;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.FunctionApps.Commands.UpdateFunctionApp;

public sealed class UpdateFunctionAppCommandValidatorTests
{
    private const string RuntimeStackProperty = nameof(UpdateFunctionAppCommand.RuntimeStack);
    private const string RuntimeVersionProperty = nameof(UpdateFunctionAppCommand.RuntimeVersion);
    private const string DeploymentModeProperty = nameof(UpdateFunctionAppCommand.DeploymentMode);

    private readonly UpdateFunctionAppCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = CreateCommand();

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_InvalidRuntimeStack_When_Validate_Then_FailsOnRuntimeStack()
    {
        // Arrange
        var command = CreateCommand(runtimeStack: "Go");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == RuntimeStackProperty);
    }

    [Fact]
    public void Given_InvalidRuntimeVersionForSelectedStack_When_Validate_Then_FailsOnRuntimeVersion()
    {
        // Arrange
        var command = CreateCommand(runtimeVersion: "99.0");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == RuntimeVersionProperty);
    }

    [Fact]
    public void Given_InvalidDeploymentMode_When_Validate_Then_FailsOnDeploymentMode()
    {
        // Arrange
        var command = CreateCommand(deploymentMode: "Zip");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == DeploymentModeProperty);
    }

    private static UpdateFunctionAppCommand CreateCommand(
        string runtimeStack = nameof(FunctionAppRuntimeStack.FunctionAppRuntimeStackEnum.DotNet),
        string runtimeVersion = "8-isolated",
        string deploymentMode = nameof(DeploymentMode.DeploymentModeType.Code))
    {
        return new UpdateFunctionAppCommand(
            AzureResourceId.CreateUnique(),
            new Name("func-app"),
            new Location(Location.LocationEnum.WestEurope),
            Guid.NewGuid(),
            runtimeStack,
            runtimeVersion,
            HttpsOnly: true,
            deploymentMode,
            ContainerRegistryId: null,
            AcrAuthMode: null,
            AcrPullIdentityId: null,
            DockerImageName: null);
    }
}
