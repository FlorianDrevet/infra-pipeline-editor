using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ContainerRegistryAggregate;

public sealed class ContainerRegistryTests
{
    private const string DefaultName = "acrprod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static ContainerRegistry CreateValidContainerRegistry(bool isExisting = false)
    {
        return ContainerRegistry.Create(
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
        var sut = ContainerRegistry.Create(
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
            (DevEnvironment, (string?)"Basic", (bool?)false, (string?)"Enabled", (bool?)false),
            (ProdEnvironment, (string?)"Premium", (bool?)false, (string?)"Disabled", (bool?)true),
        };

        // Act
        var sut = ContainerRegistry.Create(
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
            (DevEnvironment, (string?)"Basic", (bool?)false, (string?)"Enabled", (bool?)false),
        };

        // Act
        var sut = ContainerRegistry.Create(
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
        var sut = CreateValidContainerRegistry();

        // Act
        sut.Update(new Name("acrupdated"), new Location(Location.LocationEnum.NorthEurope));

        // Assert
        sut.Name.Value.Should().Be("acrupdated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
    }

    // ─── SetEnvironmentSettings ─────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidContainerRegistry();

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "Basic", true, "Enabled", false);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.EnvironmentName.Should().Be(DevEnvironment);
        entry.Sku.Should().Be("Basic");
        entry.AdminUserEnabled.Should().BeTrue();
        entry.PublicNetworkAccess.Should().Be("Enabled");
        entry.ZoneRedundancy.Should().BeFalse();
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidContainerRegistry();
        sut.SetEnvironmentSettings(DevEnvironment, "Basic", true, "Enabled", false);

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "Premium", false, "Disabled", true);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.Sku.Should().Be("Premium");
        entry.AdminUserEnabled.Should().BeFalse();
        entry.PublicNetworkAccess.Should().Be("Disabled");
        entry.ZoneRedundancy.Should().BeTrue();
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidContainerRegistry(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "Basic", true, "Enabled", false);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidContainerRegistry();
        sut.SetEnvironmentSettings(DevEnvironment, "Basic", true, "Enabled", false);
        var settings = new[]
        {
            ("staging", (string?)"Standard", (bool?)false, (string?)"Enabled", (bool?)false),
            (ProdEnvironment, (string?)"Premium", (bool?)false, (string?)"Disabled", (bool?)true),
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
        var sut = CreateValidContainerRegistry(isExisting: true);
        var settings = new[]
        {
            (DevEnvironment, (string?)"Basic", (bool?)false, (string?)"Enabled", (bool?)false),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
