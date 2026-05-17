using FluentAssertions;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;

public sealed class UpdateNetworkSecurityGroupCommandValidatorTests
{
    private readonly UpdateNetworkSecurityGroupCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new UpdateNetworkSecurityGroupCommand(
            AzureResourceId.CreateUnique(),
            new Name("my-nsg"),
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
        var command = new UpdateNetworkSecurityGroupCommand(
            null!,
            new Name("my-nsg"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateNetworkSecurityGroupCommand.Id));
    }

    [Fact]
    public void Given_EmptyName_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new UpdateNetworkSecurityGroupCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateNetworkSecurityGroupCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_FailsOnLocation()
    {
        // Arrange
        var command = new UpdateNetworkSecurityGroupCommand(
            AzureResourceId.CreateUnique(),
            new Name("my-nsg"),
            null!);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateNetworkSecurityGroupCommand.Location));
    }
}
