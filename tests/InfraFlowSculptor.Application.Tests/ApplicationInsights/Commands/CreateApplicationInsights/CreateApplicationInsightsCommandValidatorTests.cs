using FluentAssertions;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ApplicationInsights.Commands.CreateApplicationInsights;

public sealed class CreateApplicationInsightsCommandValidatorTests
{
    private readonly CreateApplicationInsightsCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new CreateApplicationInsightsCommand(
            ResourceGroupId.CreateUnique(),
            new Name("app-insights"),
            new Location(Location.LocationEnum.WestEurope),
            Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_Fails()
    {
        var command = new CreateApplicationInsightsCommand(
            ResourceGroupId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope),
            Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateApplicationInsightsCommand.Name));
    }

    [Fact]
    public void Given_NullResourceGroupId_When_Validate_Then_Fails()
    {
        var command = new CreateApplicationInsightsCommand(
            null!,
            new Name("app-insights"),
            new Location(Location.LocationEnum.WestEurope),
            Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateApplicationInsightsCommand.ResourceGroupId));
    }

    [Fact]
    public void Given_EmptyLogAnalyticsWorkspaceId_When_Validate_Then_Fails()
    {
        var command = new CreateApplicationInsightsCommand(
            ResourceGroupId.CreateUnique(),
            new Name("app-insights"),
            new Location(Location.LocationEnum.WestEurope),
            Guid.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateApplicationInsightsCommand.LogAnalyticsWorkspaceId));
    }
}
