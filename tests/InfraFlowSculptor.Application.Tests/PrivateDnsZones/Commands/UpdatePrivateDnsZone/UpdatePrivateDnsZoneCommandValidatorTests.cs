using FluentAssertions;
using InfraFlowSculptor.Application.PrivateDnsZones.Commands.UpdatePrivateDnsZone;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.PrivateDnsZones.Commands.UpdatePrivateDnsZone;

public sealed class UpdatePrivateDnsZoneCommandValidatorTests
{
    private readonly UpdatePrivateDnsZoneCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdatePrivateDnsZoneCommand(
            AzureResourceId.CreateUnique(),
            new Name("privatelink.database.windows.net"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyId_When_Validate_Then_FailsOnId()
    {
        // Arrange
        var command = new UpdatePrivateDnsZoneCommand(
            null!,
            new Name("privatelink.database.windows.net"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePrivateDnsZoneCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new UpdatePrivateDnsZoneCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePrivateDnsZoneCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        // Arrange
        var command = new UpdatePrivateDnsZoneCommand(
            AzureResourceId.CreateUnique(),
            new Name("privatelink.database.windows.net"),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePrivateDnsZoneCommand.Location));
    }
}
