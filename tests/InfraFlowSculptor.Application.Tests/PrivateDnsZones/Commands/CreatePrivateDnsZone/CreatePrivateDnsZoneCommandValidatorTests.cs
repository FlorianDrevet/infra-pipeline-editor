using FluentAssertions;
using InfraFlowSculptor.Application.PrivateDnsZones.Commands.CreatePrivateDnsZone;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.PrivateDnsZones.Commands.CreatePrivateDnsZone;

public sealed class CreatePrivateDnsZoneCommandValidatorTests
{
    private readonly CreatePrivateDnsZoneCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreatePrivateDnsZoneCommand(
            ResourceGroupId.CreateUnique(),
            new Name("privatelink.database.windows.net"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        // Arrange
        var command = new CreatePrivateDnsZoneCommand(
            null!,
            new Name("privatelink.database.windows.net"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePrivateDnsZoneCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new CreatePrivateDnsZoneCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePrivateDnsZoneCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        // Arrange
        var command = new CreatePrivateDnsZoneCommand(
            ResourceGroupId.CreateUnique(),
            new Name("privatelink.database.windows.net"),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePrivateDnsZoneCommand.Location));
    }
}
