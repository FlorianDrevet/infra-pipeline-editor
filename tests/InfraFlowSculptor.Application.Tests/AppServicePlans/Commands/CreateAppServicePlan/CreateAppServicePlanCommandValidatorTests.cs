using FluentAssertions;
using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppServicePlans.Commands.CreateAppServicePlan;

public sealed class CreateAppServicePlanCommandValidatorTests
{
    private readonly CreateAppServicePlanCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new CreateAppServicePlanCommand(
            ResourceGroupId.CreateUnique(),
            new Name("asp-test"),
            new Location(Location.LocationEnum.WestEurope),
            "Windows");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_Fails()
    {
        var command = new CreateAppServicePlanCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope),
            "Windows");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppServicePlanCommand.Name));
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_Fails()
    {
        var command = new CreateAppServicePlanCommand(
            null!,
            new Name("asp-test"),
            new Location(Location.LocationEnum.WestEurope),
            "Windows");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppServicePlanCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptyOsType_When_Validate_Then_Fails()
    {
        var command = new CreateAppServicePlanCommand(
            ResourceGroupId.CreateUnique(),
            new Name("asp-test"),
            new Location(Location.LocationEnum.WestEurope),
            "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppServicePlanCommand.OsType));
    }

    [Fact]
    public void Given_InvalidOsType_When_Validate_Then_Fails()
    {
        var command = new CreateAppServicePlanCommand(
            ResourceGroupId.CreateUnique(),
            new Name("asp-test"),
            new Location(Location.LocationEnum.WestEurope),
            "MacOS");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateAppServicePlanCommand.OsType));
    }
}
