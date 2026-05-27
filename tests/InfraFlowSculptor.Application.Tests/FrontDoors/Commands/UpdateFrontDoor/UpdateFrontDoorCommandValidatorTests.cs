using FluentAssertions;
using InfraFlowSculptor.Application.FrontDoors.Commands.UpdateFrontDoor;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.FrontDoors.Commands.UpdateFrontDoor;

public sealed class UpdateFrontDoorCommandValidatorTests
{
    private readonly UpdateFrontDoorCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateFrontDoorCommand(
            AzureResourceId.CreateUnique(),
            new Name("updated-frontdoor"),
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
        var command = new UpdateFrontDoorCommand(
            null!,
            new Name("updated-frontdoor"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFrontDoorCommand.Id));
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new UpdateFrontDoorCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFrontDoorCommand.Name));
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_FailsOnLocation()
    {
        // Arrange
        var command = new UpdateFrontDoorCommand(
            AzureResourceId.CreateUnique(),
            new Name("updated-frontdoor"),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFrontDoorCommand.Location));
    }
}
