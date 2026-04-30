using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.LogAnalyticsWorkspaceAggregate;

public sealed class LogAnalyticsWorkspaceTests
{
    private const string DefaultName = "law-prod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static LogAnalyticsWorkspace CreateValid(bool isExisting = false)
    {
        return LogAnalyticsWorkspace.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();

        // Act
        var sut = LogAnalyticsWorkspace.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue));

        // Assert
        sut.Id.Should().NotBeNull();
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (string?)"PerGB2018", (int?)30, (decimal?)1m),
            (ProdEnvironment, (string?)"PerGB2018", (int?)90, (decimal?)10m),
        };

        // Act
        var sut = LogAnalyticsWorkspace.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
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
            (DevEnvironment, (string?)"PerGB2018", (int?)30, (decimal?)1m),
        };

        // Act
        var sut = LogAnalyticsWorkspace.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsNameAndLocation()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.Update(new Name("law-updated"), new Location(Location.LocationEnum.NorthEurope));

        // Assert
        sut.Name.Value.Should().Be("law-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
    }

    // ─── SetEnvironmentSettings ────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValid();

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "PerGB2018", 90, 10m);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntry()
    {
        // Arrange
        var sut = CreateValid();
        sut.SetEnvironmentSettings(ProdEnvironment, "PerGB2018", 30, 1m);

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "PerGB2018", 90, 10m);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.RetentionInDays.Should().Be(90);
        entry.DailyQuotaGb.Should().Be(10m);
    }

    [Fact]
    public void Given_IsExisting_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValid(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(ProdEnvironment, "PerGB2018", 90, 10m);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
