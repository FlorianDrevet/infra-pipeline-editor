using FluentAssertions;
using InfraFlowSculptor.Application.PrivateEndpoints.Commands.RemovePrivateEndpoint;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.PrivateEndpoints.Commands.RemovePrivateEndpoint;

public sealed class RemovePrivateEndpointCommandValidatorTests
{
    private readonly RemovePrivateEndpointCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemovePrivateEndpointCommand(
            AzureResourceId.CreateUnique(),
            PrivateEndpointConfigId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceId_When_Validate_Then_FailsOnResourceId()
    {
        // Arrange
        var command = new RemovePrivateEndpointCommand(
            null!,
            PrivateEndpointConfigId.CreateUnique());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemovePrivateEndpointCommand.ResourceId));
    }

    [Fact]
    public void Given_EmptyConfigId_When_Validate_Then_FailsOnConfigId()
    {
        // Arrange
        var command = new RemovePrivateEndpointCommand(
            AzureResourceId.CreateUnique(),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemovePrivateEndpointCommand.ConfigId));
    }
}
