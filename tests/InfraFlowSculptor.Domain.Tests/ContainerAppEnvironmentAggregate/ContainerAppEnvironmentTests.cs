using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ContainerAppEnvironmentAggregate;

public sealed class ContainerAppEnvironmentTests
{
    private const string DefaultName = "cae-prod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static ContainerAppEnvironment CreateValidContainerAppEnvironment(
        bool isExisting = false,
        AzureResourceId? logAnalyticsWorkspaceId = null)
    {
        return ContainerAppEnvironment.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            logAnalyticsWorkspaceId,
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();
        var workspaceId = AzureResourceId.CreateUnique();

        // Act
        var sut = ContainerAppEnvironment.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            workspaceId);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.LogAnalyticsWorkspaceId.Should().Be(workspaceId);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NoOptionalArgs_When_Create_Then_DefaultsApplied()
    {
        // Act
        var sut = ContainerAppEnvironment.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue));

        // Assert
        sut.LogAnalyticsWorkspaceId.Should().BeNull();
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (string?)"Consumption", (string?)"D4", (bool?)false, (bool?)false),
            (ProdEnvironment, (string?)"Premium", (string?)"D8", (bool?)true, (bool?)true),
        };

        // Act
        var sut = ContainerAppEnvironment.Create(
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
            (DevEnvironment, (string?)"Consumption", (string?)"D4", (bool?)false, (bool?)false),
        };

        // Act
        var sut = ContainerAppEnvironment.Create(
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
    public void Given_NewValues_When_Update_Then_AssignsAllProperties()
    {
        // Arrange
        var sut = CreateValidContainerAppEnvironment();
        var newWorkspaceId = AzureResourceId.CreateUnique();

        // Act
        sut.Update(
            new Name("cae-renamed"),
            new Location(Location.LocationEnum.NorthEurope),
            newWorkspaceId);

        // Assert
        sut.Name.Value.Should().Be("cae-renamed");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.LogAnalyticsWorkspaceId.Should().Be(newWorkspaceId);
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyChangesNameAndLocation()
    {
        // Arrange
        var originalWorkspaceId = AzureResourceId.CreateUnique();
        var sut = CreateValidContainerAppEnvironment(isExisting: true, logAnalyticsWorkspaceId: originalWorkspaceId);

        // Act
        sut.Update(
            new Name("renamed"),
            new Location(Location.LocationEnum.NorthEurope),
            AzureResourceId.CreateUnique());

        // Assert
        sut.Name.Value.Should().Be("renamed");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.LogAnalyticsWorkspaceId.Should().Be(originalWorkspaceId);
    }

    // ─── SetEnvironmentSettings ─────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidContainerAppEnvironment();

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "Consumption", "D4", false, false);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.EnvironmentName.Should().Be(DevEnvironment);
        entry.Sku.Should().Be("Consumption");
        entry.WorkloadProfileType.Should().Be("D4");
        entry.InternalLoadBalancerEnabled.Should().BeFalse();
        entry.ZoneRedundancyEnabled.Should().BeFalse();
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidContainerAppEnvironment();
        sut.SetEnvironmentSettings(DevEnvironment, "Consumption", "D4", false, false);

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "Premium", "D8", true, true);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.Sku.Should().Be("Premium");
        entry.WorkloadProfileType.Should().Be("D8");
        entry.InternalLoadBalancerEnabled.Should().BeTrue();
        entry.ZoneRedundancyEnabled.Should().BeTrue();
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidContainerAppEnvironment(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(DevEnvironment, "Consumption", "D4", false, false);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidContainerAppEnvironment();
        sut.SetEnvironmentSettings(DevEnvironment, "Consumption", "D4", false, false);
        var settings = new[]
        {
            ("staging", (string?)"Premium", (string?)"D4", (bool?)true, (bool?)false),
            (ProdEnvironment, (string?)"Premium", (string?)"D8", (bool?)true, (bool?)true),
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
        var sut = CreateValidContainerAppEnvironment(isExisting: true);
        var settings = new[]
        {
            (DevEnvironment, (string?)"Consumption", (string?)"D4", (bool?)false, (bool?)false),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
