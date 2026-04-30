using FluentAssertions;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ApplicationInsightsAggregate;

public sealed class ApplicationInsightsTests
{
    private const string DefaultName = "appi-prod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static ApplicationInsights CreateValid(bool isExisting = false)
    {
        return ApplicationInsights.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();
        var lawId = AzureResourceId.CreateUnique();

        // Act
        var sut = ApplicationInsights.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            lawId);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.LogAnalyticsWorkspaceId.Should().Be(lawId);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (decimal?)50m, (int?)30, (bool?)false, (bool?)false, (string?)"ApplicationInsights"),
            (ProdEnvironment, (decimal?)100m, (int?)90, (bool?)true, (bool?)true, (string?)"LogAnalytics"),
        };

        // Act
        var sut = ApplicationInsights.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            environmentSettings: settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IgnoresEnvironmentSettings()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (decimal?)50m, (int?)30, (bool?)false, (bool?)false, (string?)"ApplicationInsights"),
        };

        // Act
        var sut = ApplicationInsights.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            environmentSettings: settings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAllProperties()
    {
        // Arrange
        var sut = CreateValid();
        var newLawId = AzureResourceId.CreateUnique();

        // Act
        sut.Update(
            new Name("appi-updated"),
            new Location(Location.LocationEnum.NorthEurope),
            newLawId);

        // Assert
        sut.Name.Value.Should().Be("appi-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.LogAnalyticsWorkspaceId.Should().Be(newLawId);
    }

    [Fact]
    public void Given_IsExisting_When_Update_Then_OnlyAssignsNameAndLocation()
    {
        // Arrange
        var sut = CreateValid(isExisting: true);
        var originalLawId = sut.LogAnalyticsWorkspaceId;
        var newLawId = AzureResourceId.CreateUnique();

        // Act
        sut.Update(
            new Name("appi-updated"),
            new Location(Location.LocationEnum.NorthEurope),
            newLawId);

        // Assert
        sut.Name.Value.Should().Be("appi-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.LogAnalyticsWorkspaceId.Should().Be(originalLawId);
    }

    // ─── SetEnvironmentSettings ────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, 100m, 90, true, true, "LogAnalytics");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntry()
    {
        // Arrange
        var sut = CreateValid();
        sut.SetEnvironmentSettings(ProdEnvironment, 50m, 30, false, false, "ApplicationInsights");

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, 100m, 90, true, true, "LogAnalytics");

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().SamplingPercentage.Should().Be(100m);
    }

    [Fact]
    public void Given_IsExisting_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValid(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, 100m, 90, true, true, "LogAnalytics");

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
