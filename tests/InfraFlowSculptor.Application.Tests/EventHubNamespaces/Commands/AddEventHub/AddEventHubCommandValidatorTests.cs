using FluentAssertions;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHub;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.AddEventHub;

public sealed class AddEventHubCommandValidatorTests
{
    private readonly AddEventHubCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddEventHubCommand(
            AzureResourceId.CreateUnique(),
            "my-event-hub");

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
        var command = new AddEventHubCommand(
            null!,
            "my-event-hub");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddEventHubCommand.EventHubNamespaceId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new AddEventHubCommand(
            AzureResourceId.CreateUnique(),
            "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddEventHubCommand.Name));
    }
}
