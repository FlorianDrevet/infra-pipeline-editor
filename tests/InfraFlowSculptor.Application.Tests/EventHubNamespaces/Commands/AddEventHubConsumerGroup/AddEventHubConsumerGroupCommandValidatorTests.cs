using FluentAssertions;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHubConsumerGroup;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.AddEventHubConsumerGroup;

public sealed class AddEventHubConsumerGroupCommandValidatorTests
{
    private readonly AddEventHubConsumerGroupCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new AddEventHubConsumerGroupCommand(
            AzureResourceId.CreateUnique(),
            "my-event-hub",
            "my-consumer-group");

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
        var command = new AddEventHubConsumerGroupCommand(
            null!,
            "my-event-hub",
            "my-consumer-group");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddEventHubConsumerGroupCommand.EventHubNamespaceId));
    }

    [Fact]
    public void Given_EmptyEventHubName_When_Validate_Then_FailsOnEventHubName()
    {
        // Arrange
        var command = new AddEventHubConsumerGroupCommand(
            AzureResourceId.CreateUnique(),
            "",
            "my-consumer-group");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddEventHubConsumerGroupCommand.EventHubName));
    }

    [Fact]
    public void Given_EmptyConsumerGroupName_When_Validate_Then_FailsOnConsumerGroupName()
    {
        // Arrange
        var command = new AddEventHubConsumerGroupCommand(
            AzureResourceId.CreateUnique(),
            "my-event-hub",
            "");

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddEventHubConsumerGroupCommand.ConsumerGroupName));
    }
}
