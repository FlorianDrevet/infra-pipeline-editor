using FluentAssertions;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.UpdateApplicationInsights;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.Tests.ApplicationInsights.Commands.UpdateApplicationInsights;

public sealed class UpdateApplicationInsightsCommandValidatorTests
{
    private readonly UpdateApplicationInsightsCommandValidator _sut = new();

    [Fact]
    public void Given_ValidCommand_When_Validate_Then_Succeeds()
    {
        var command = new UpdateApplicationInsightsCommand(
            AzureResourceId.CreateUnique(),
            new Name("app-insights"),
            new Location(Location.LocationEnum.WestEurope),
            Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_NullId_When_Validate_Then_Fails()
    {
        var command = new UpdateApplicationInsightsCommand(
            null!,
            new Name("app-insights"),
            new Location(Location.LocationEnum.WestEurope),
            Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateApplicationInsightsCommand.Id));
    }

    [Fact]
    public void Given_NullName_When_Validate_Then_Fails()
    {
        var command = new UpdateApplicationInsightsCommand(
            AzureResourceId.CreateUnique(),
            null!,
            new Location(Location.LocationEnum.WestEurope),
            Guid.NewGuid());

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateApplicationInsightsCommand.Name));
    }

    [Fact]
    public void Given_EmptyLogAnalyticsWorkspaceId_When_Validate_Then_Fails()
    {
        var command = new UpdateApplicationInsightsCommand(
            AzureResourceId.CreateUnique(),
            new Name("app-insights"),
            new Location(Location.LocationEnum.WestEurope),
            Guid.Empty);

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateApplicationInsightsCommand.LogAnalyticsWorkspaceId));
    }
}
