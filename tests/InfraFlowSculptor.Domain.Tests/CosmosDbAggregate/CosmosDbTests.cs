using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.CosmosDbAggregate;

public sealed class CosmosDbTests
{
    private const string DefaultName = "cosmos-prod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static CosmosDb CreateValidCosmosDb(bool isExisting = false)
    {
        return CosmosDb.Create(
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
        var sut = CosmosDb.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue));

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
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
            (DevEnvironment, (string?)"SQL", (string?)"Session", (int?)100, (int?)5, (bool?)false, (bool?)false, (string?)"Periodic", (bool?)true),
            (ProdEnvironment, (string?)"SQL", (string?)"Strong", (int?)null, (int?)null, (bool?)true, (bool?)true, (string?)"Continuous", (bool?)false),
        };

        // Act
        var sut = CosmosDb.Create(
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
            (DevEnvironment, (string?)"SQL", (string?)"Session", (int?)100, (int?)5, (bool?)false, (bool?)false, (string?)"Periodic", (bool?)true),
        };

        // Act
        var sut = CosmosDb.Create(
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
        var sut = CreateValidCosmosDb();

        // Act
        sut.Update(new Name("cosmos-updated"), new Location(Location.LocationEnum.NorthEurope));

        // Assert
        sut.Name.Value.Should().Be("cosmos-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
    }

    // ─── SetEnvironmentSettings ─────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidCosmosDb();

        // Act
        sut.SetEnvironmentSettings(
            DevEnvironment, "SQL", "BoundedStaleness", 100, 5, false, false, "Periodic", true);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.EnvironmentName.Should().Be(DevEnvironment);
        entry.DatabaseApiType.Should().Be("SQL");
        entry.ConsistencyLevel.Should().Be("BoundedStaleness");
        entry.MaxStalenessPrefix.Should().Be(100);
        entry.MaxIntervalInSeconds.Should().Be(5);
        entry.EnableAutomaticFailover.Should().BeFalse();
        entry.EnableMultipleWriteLocations.Should().BeFalse();
        entry.BackupPolicyType.Should().Be("Periodic");
        entry.EnableFreeTier.Should().BeTrue();
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidCosmosDb();
        sut.SetEnvironmentSettings(DevEnvironment, "SQL", "Session", null, null, false, false, "Periodic", true);

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "MongoDB", "Strong", null, null, true, true, "Continuous", false);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.DatabaseApiType.Should().Be("MongoDB");
        entry.ConsistencyLevel.Should().Be("Strong");
        entry.EnableAutomaticFailover.Should().BeTrue();
        entry.EnableMultipleWriteLocations.Should().BeTrue();
        entry.BackupPolicyType.Should().Be("Continuous");
        entry.EnableFreeTier.Should().BeFalse();
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidCosmosDb(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "SQL", "Session", null, null, false, false, "Periodic", true);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidCosmosDb();
        sut.SetEnvironmentSettings(DevEnvironment, "SQL", "Session", null, null, false, false, "Periodic", true);
        var settings = new[]
        {
            ("staging", (string?)"SQL", (string?)"Eventual", (int?)null, (int?)null, (bool?)false, (bool?)false, (string?)"Periodic", (bool?)false),
            (ProdEnvironment, (string?)"SQL", (string?)"Strong", (int?)null, (int?)null, (bool?)true, (bool?)true, (string?)"Continuous", (bool?)false),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Should().NotContain(es => es.EnvironmentName == DevEnvironment);
    }

    [Fact]
    public void Given_IsExistingResource_When_SetAllEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidCosmosDb(isExisting: true);
        var settings = new[]
        {
            (DevEnvironment, (string?)"SQL", (string?)"Session", (int?)null, (int?)null, (bool?)false, (bool?)false, (string?)"Periodic", (bool?)true),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
