using FluentAssertions;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHubConsumerGroup;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.RemoveEventHubConsumerGroup;

public sealed class RemoveEventHubConsumerGroupCommandValidatorTests
{
    private readonly RemoveEventHubConsumerGroupCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new RemoveEventHubConsumerGroupCommand(
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
        var command = new RemoveEventHubConsumerGroupCommand(
            null!,
            Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveEventHubConsumerGroupCommand.EventHubNamespaceId));
    }

    [Fact]
    public void Given_EmptyConsumerGroupId_When_Validate_Then_FailsOnConsumerGroupId()
    {
        // Arrange
        var command = new RemoveEventHubConsumerGroupCommand(
            AzureResourceId.CreateUnique(),
            Guid.Empty);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RemoveEventHubConsumerGroupCommand.ConsumerGroupId));
    }
}
