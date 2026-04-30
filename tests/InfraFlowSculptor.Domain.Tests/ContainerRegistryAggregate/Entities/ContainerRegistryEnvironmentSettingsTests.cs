using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ContainerRegistryAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.ContainerRegistryAggregate.Entities;

public sealed class ContainerRegistryEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var registryId = AzureResourceId.CreateUnique();

        // Act
        var sut = ContainerRegistryEnvironmentSettings.Create(
            registryId,
            EnvironmentName,
            sku: "Premium",
            adminUserEnabled: true,
            publicNetworkAccess: "Disabled",
            zoneRedundancy: true);

        // Assert
        sut.ContainerRegistryId.Should().Be(registryId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku.Should().Be("Premium");
        sut.AdminUserEnabled.Should().BeTrue();
        sut.PublicNetworkAccess.Should().Be("Disabled");
        sut.ZoneRedundancy.Should().BeTrue();
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = ContainerRegistryEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            sku: "Basic", adminUserEnabled: false,
            publicNetworkAccess: "Enabled", zoneRedundancy: false);

        // Act
        sut.Update(sku: "Premium", adminUserEnabled: true,
            publicNetworkAccess: "Disabled", zoneRedundancy: true);

        // Assert
        sut.Sku.Should().Be("Premium");
        sut.AdminUserEnabled.Should().BeTrue();
        sut.PublicNetworkAccess.Should().Be("Disabled");
        sut.ZoneRedundancy.Should().BeTrue();
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = ContainerRegistryEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            sku: null, adminUserEnabled: null, publicNetworkAccess: null, zoneRedundancy: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = ContainerRegistryEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            sku: "Premium", adminUserEnabled: true,
            publicNetworkAccess: "Disabled", zoneRedundancy: false);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["sku"].Should().Be("Premium");
        dict["adminUserEnabled"].Should().Be("true");
        dict["publicNetworkAccess"].Should().Be("Disabled");
        dict["zoneRedundancy"].Should().Be("false");
    }
}
