using FluentAssertions;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.AddServiceBusTopicSubscription;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ServiceBusNamespaces.Commands.AddServiceBusTopicSubscription;

public sealed class AddServiceBusTopicSubscriptionCommandValidatorTests
{
    private readonly AddServiceBusTopicSubscriptionCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddServiceBusTopicSubscriptionCommand(
            AzureResourceId.CreateUnique(),
            "my-topic",
            "my-subscription");

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
        var command = new AddServiceBusTopicSubscriptionCommand(
            null!,
            "my-topic",
            "my-subscription");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddServiceBusTopicSubscriptionCommand.ServiceBusNamespaceId));
    }

    [Fact]
    public void Given_EmptyTopicName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AddServiceBusTopicSubscriptionCommand(
            AzureResourceId.CreateUnique(),
            "",
            "my-subscription");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddServiceBusTopicSubscriptionCommand.TopicName));
    }

    [Fact]
    public void Given_EmptySubscriptionName_When_Validate_Then_Fails()
    {
        // Arrange
        var command = new AddServiceBusTopicSubscriptionCommand(
            AzureResourceId.CreateUnique(),
            "my-topic",
            "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddServiceBusTopicSubscriptionCommand.SubscriptionName));
    }
}
