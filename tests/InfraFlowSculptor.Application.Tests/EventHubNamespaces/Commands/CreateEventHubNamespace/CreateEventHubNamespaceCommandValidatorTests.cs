using FluentAssertions;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.CreateEventHubNamespace;

public sealed class CreateEventHubNamespaceCommandValidatorTests
{
    private readonly CreateEventHubNamespaceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateEventHubNamespaceCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-ehns"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new CreateEventHubNamespaceCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEventHubNamespaceCommand.Name));
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        // Arrange
        var command = new CreateEventHubNamespaceCommand(
            null!,
            new Name("my-ehns"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEventHubNamespaceCommand.ResourceGroupId));
    }
}
