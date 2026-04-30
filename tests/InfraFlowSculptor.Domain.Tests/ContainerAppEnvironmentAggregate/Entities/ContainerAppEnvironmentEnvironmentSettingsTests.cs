using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.ContainerAppEnvironmentAggregate.Entities;

public sealed class ContainerAppEnvironmentEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var caeId = AzureResourceId.CreateUnique();

        // Act
        var sut = ContainerAppEnvironmentEnvironmentSettings.Create(
            caeId,
            EnvironmentName,
            sku: "Premium",
            workloadProfileType: "D8",
            internalLoadBalancerEnabled: true,
            zoneRedundancyEnabled: true);

        // Assert
        sut.ContainerAppEnvironmentId.Should().Be(caeId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku.Should().Be("Premium");
        sut.WorkloadProfileType.Should().Be("D8");
        sut.InternalLoadBalancerEnabled.Should().BeTrue();
        sut.ZoneRedundancyEnabled.Should().BeTrue();
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = ContainerAppEnvironmentEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            sku: "Consumption", workloadProfileType: "D4",
            internalLoadBalancerEnabled: false, zoneRedundancyEnabled: false);

        // Act
        sut.Update(sku: "Premium", workloadProfileType: "D8",
            internalLoadBalancerEnabled: true, zoneRedundancyEnabled: true);

        // Assert
        sut.Sku.Should().Be("Premium");
        sut.WorkloadProfileType.Should().Be("D8");
        sut.InternalLoadBalancerEnabled.Should().BeTrue();
        sut.ZoneRedundancyEnabled.Should().BeTrue();
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = ContainerAppEnvironmentEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            sku: null, workloadProfileType: null,
            internalLoadBalancerEnabled: null, zoneRedundancyEnabled: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = ContainerAppEnvironmentEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            sku: "Premium", workloadProfileType: "D8",
            internalLoadBalancerEnabled: true, zoneRedundancyEnabled: false);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["sku"].Should().Be("Premium");
        dict["workloadProfileType"].Should().Be("D8");
        dict["internalLoadBalancerEnabled"].Should().Be("true");
        dict["zoneRedundancyEnabled"].Should().Be("false");
    }
}
