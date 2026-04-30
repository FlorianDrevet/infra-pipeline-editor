using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.ServiceBusNamespaceAggregate.Entities;

public sealed class ServiceBusNamespaceEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var sbId = AzureResourceId.CreateUnique();

        // Act
        var sut = ServiceBusNamespaceEnvironmentSettings.Create(
            sbId, EnvironmentName, "Premium", 2, true, true, "1.2");

        // Assert
        sut.ServiceBusNamespaceId.Should().Be(sbId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku.Should().Be("Premium");
        sut.Capacity.Should().Be(2);
        sut.ZoneRedundant.Should().BeTrue();
        sut.DisableLocalAuth.Should().BeTrue();
        sut.MinimumTlsVersion.Should().Be("1.2");
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = ServiceBusNamespaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, "Standard", null, false, false, "1.0");

        // Act
        sut.Update("Premium", 4, true, true, "1.2");

        // Assert
        sut.Sku.Should().Be("Premium");
        sut.Capacity.Should().Be(4);
        sut.ZoneRedundant.Should().BeTrue();
        sut.DisableLocalAuth.Should().BeTrue();
        sut.MinimumTlsVersion.Should().Be("1.2");
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = ServiceBusNamespaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, null, null, null, null, null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = ServiceBusNamespaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, "Premium", 2, true, false, "1.2");

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["sku"].Should().Be("Premium");
        dict["capacity"].Should().Be("2");
        dict["zoneRedundant"].Should().Be("true");
        dict["disableLocalAuth"].Should().Be("false");
        dict["minimumTlsVersion"].Should().Be("1.2");
    }
}
