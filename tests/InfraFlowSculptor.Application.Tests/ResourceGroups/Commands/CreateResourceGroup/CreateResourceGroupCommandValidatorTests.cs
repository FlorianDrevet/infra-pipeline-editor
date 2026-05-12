using FluentAssertions;
using InfraFlowSculptor.Application.ResourceGroup.Commands.CreateResourceGroup;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ResourceGroups.Commands.CreateResourceGroup;

public sealed class CreateResourceGroupCommandValidatorTests
{
    private const string NameProperty = nameof(CreateResourceGroupCommand.Name);

    private readonly CreateResourceGroupCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        // Arrange
        var command = new CreateResourceGroupCommand(
            InfrastructureConfigId.CreateUnique(),
            new Name("rg-shared"),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NameLongerThan90Characters_When_Validate_Then_FailsOnName()
    {
        // Arrange
        var command = new CreateResourceGroupCommand(
            InfrastructureConfigId.CreateUnique(),
            new Name(new string('r', 91)),
            new Location(Location.LocationEnum.WestEurope));

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == NameProperty);
    }
}