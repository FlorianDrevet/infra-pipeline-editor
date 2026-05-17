using FluentAssertions;
using InfraFlowSculptor.Application.AppServicePlans.Commands.UpdateAppServicePlan;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.AppServicePlans.Commands.UpdateAppServicePlan;

public sealed class UpdateAppServicePlanCommandValidatorTests
{
    private readonly UpdateAppServicePlanCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new UpdateAppServicePlanCommand(
            AzureResourceId.CreateUnique(),
            new Name("asp-test"),
            new Location(Location.LocationEnum.WestEurope),
            "Linux");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_Fails()
    {
        var command = new UpdateAppServicePlanCommand(
            null!,
            new Name("asp-test"),
            new Location(Location.LocationEnum.WestEurope),
            "Linux");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAppServicePlanCommand.Id));
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_Fails()
    {
        var command = new UpdateAppServicePlanCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope),
            "Linux");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAppServicePlanCommand.Name));
    }

    [Fact]
    public void Given_EmptyLocation_When_Validate_Then_Fails()
    {
        var command = new UpdateAppServicePlanCommand(
            AzureResourceId.CreateUnique(),
            new Name("asp-test"),
            null!,
            "Linux");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAppServicePlanCommand.Location));
    }

    [Fact]
    public void Given_EmptyOsType_When_Validate_Then_Fails()
    {
        var command = new UpdateAppServicePlanCommand(
            AzureResourceId.CreateUnique(),
            new Name("asp-test"),
            new Location(Location.LocationEnum.WestEurope),
            "");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAppServicePlanCommand.OsType));
    }

    [Fact]
    public void Given_InvalidOsType_When_Validate_Then_Fails()
    {
        var command = new UpdateAppServicePlanCommand(
            AzureResourceId.CreateUnique(),
            new Name("asp-test"),
            new Location(Location.LocationEnum.WestEurope),
            "MacOS");

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateAppServicePlanCommand.OsType));
    }
}
