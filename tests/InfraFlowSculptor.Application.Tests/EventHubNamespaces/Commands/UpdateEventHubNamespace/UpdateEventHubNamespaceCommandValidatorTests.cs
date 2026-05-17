using FluentAssertions;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.UpdateEventHubNamespace;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.EventHubNamespaces.Commands.UpdateEventHubNamespace;

public sealed class UpdateEventHubNamespaceCommandValidatorTests
{
    private readonly UpdateEventHubNamespaceCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateEventHubNamespaceCommand(
            AzureResourceId.CreateUnique(),
            new Name("updated-ehns"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new UpdateEventHubNamespaceCommand(
            null!,
            new Name("updated-ehns"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEventHubNamespaceCommand.Id));
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new UpdateEventHubNamespaceCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEventHubNamespaceCommand.Name));
    }
}
