using FluentAssertions;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHub;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.RemoveEventHub;

public sealed class RemoveEventHubCommandValidatorTests
{
    private readonly RemoveEventHubCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveEventHubCommand(
            AzureResourceId.CreateUnique(),
            Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullEventHubNamespaceId_When_Validate_Then_FailsOnEventHubNamespaceId()
    {
        // Arrange
        var command = new RemoveEventHubCommand(
            null!,
            Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveEventHubCommand.EventHubNamespaceId));
    }

    [Fact]
    public void Given_EmptyEventHubId_When_Validate_Then_FailsOnEventHubId()
    {
        // Arrange
        var command = new RemoveEventHubCommand(
            AzureResourceId.CreateUnique(),
            Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveEventHubCommand.EventHubId));
    }
}
