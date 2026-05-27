using FluentAssertions;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusQueue;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.RemoveServiceBusQueue;

public sealed class RemoveServiceBusQueueCommandValidatorTests
{
    private readonly RemoveServiceBusQueueCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveServiceBusQueueCommand(
            AzureResourceId.CreateUnique(),
            Guid.NewGuid());

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
        var command = new RemoveServiceBusQueueCommand(
            null!,
            Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveServiceBusQueueCommand.ServiceBusNamespaceId));
    }

    [Fact]
    public void Given_EmptyQueueId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new RemoveServiceBusQueueCommand(
            AzureResourceId.CreateUnique(),
            Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveServiceBusQueueCommand.QueueId));
    }
}
