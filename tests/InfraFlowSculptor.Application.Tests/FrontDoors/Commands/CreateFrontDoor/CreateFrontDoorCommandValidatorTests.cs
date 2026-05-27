using FluentAssertions;
using InfraFlowSculptor.Application.FrontDoors.Commands.CreateFrontDoor;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.FrontDoors.Commands.CreateFrontDoor;

public sealed class CreateFrontDoorCommandValidatorTests
{
    private readonly CreateFrontDoorCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateFrontDoorCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-frontdoor"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        // Arrange
        var command = new CreateFrontDoorCommand(
            null!,
            new Name("my-frontdoor"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFrontDoorCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new CreateFrontDoorCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFrontDoorCommand.Name));
    }

    [Fact]
    public void Given_NullLocation_When_Validate_Then_FailsOnLocation()
    {
        // Arrange
        var command = new CreateFrontDoorCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-frontdoor"),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFrontDoorCommand.Location));
    }
}
