using FluentAssertions;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusQueue;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.AddServiceBusQueue;

public sealed class AddServiceBusQueueCommandValidatorTests
{
    private readonly AddServiceBusQueueCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddServiceBusQueueCommand(
            AzureResourceId.CreateUnique(),
            "my-queue");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyServiceBusNamespaceId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AddServiceBusQueueCommand(
            null!,
            "my-queue");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddServiceBusQueueCommand.ServiceBusNamespaceId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AddServiceBusQueueCommand(
            AzureResourceId.CreateUnique(),
            "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddServiceBusQueueCommand.Name));
    }
}
