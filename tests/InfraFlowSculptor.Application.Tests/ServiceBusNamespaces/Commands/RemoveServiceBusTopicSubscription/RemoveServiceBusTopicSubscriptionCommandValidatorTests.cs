using FluentAssertions;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.RemoveServiceBusTopicSubscription;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.RemoveServiceBusTopicSubscription;

public sealed class RemoveServiceBusTopicSubscriptionCommandValidatorTests
{
    private readonly RemoveServiceBusTopicSubscriptionCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveServiceBusTopicSubscriptionCommand(
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
        var command = new RemoveServiceBusTopicSubscriptionCommand(
            null!,
            Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveServiceBusTopicSubscriptionCommand.ServiceBusNamespaceId));
    }

    [Fact]
    public void Given_EmptySubscriptionId_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new RemoveServiceBusTopicSubscriptionCommand(
            AzureResourceId.CreateUnique(),
            Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveServiceBusTopicSubscriptionCommand.SubscriptionId));
    }
}
