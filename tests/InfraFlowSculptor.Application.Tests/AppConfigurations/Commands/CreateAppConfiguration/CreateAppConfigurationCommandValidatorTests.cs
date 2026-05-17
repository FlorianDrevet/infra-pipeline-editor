using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppConfigurations.Commands.CreateAppConfiguration;

public sealed class CreateAppConfigurationCommandValidatorTests
{
    private readonly CreateAppConfigurationCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new CreateAppConfigurationCommand(
            ResourceGroupId.CreateUnique(),
            new Name("my-config"),
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_FailsOnName()
    {
        var command = new CreateAppConfigurationCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppConfigurationCommand.Name));
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_FailsOnResourceGroupId()
    {
        var command = new CreateAppConfigurationCommand(
            null!,
            new Name("my-config"),
            new Location(Location.LocationEnum.WestEurope));

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppConfigurationCommand.ResourceGroupId));
    }
}
