using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.EventHubNamespaceAggregate.Entities;

public sealed class EventHubNamespaceEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var nsId = AzureResourceId.CreateUnique();

        // Act
        var sut = EventHubNamespaceEnvironmentSettings.Create(
            nsId, EnvironmentName, "Premium", 4, true, true, "1.2", true, 10);

        // Assert
        sut.EventHubNamespaceId.Should().Be(nsId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku.Should().Be("Premium");
        sut.Capacity.Should().Be(4);
        sut.ZoneRedundant.Should().BeTrue();
        sut.DisableLocalAuth.Should().BeTrue();
        sut.MinimumTlsVersion.Should().Be("1.2");
        sut.AutoInflateEnabled.Should().BeTrue();
        sut.MaxThroughputUnits.Should().Be(10);
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = EventHubNamespaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, "Standard", 1, false, false, "1.0", false, null);

        // Act
        sut.Update("Premium", 4, true, true, "1.2", true, 10);

        // Assert
        sut.Sku.Should().Be("Premium");
        sut.Capacity.Should().Be(4);
        sut.AutoInflateEnabled.Should().BeTrue();
        sut.MaxThroughputUnits.Should().Be(10);
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = EventHubNamespaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, null, null, null, null, null, null, null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = EventHubNamespaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, "Premium", 4, true, false, "1.2", true, 10);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["sku"].Should().Be("Premium");
        dict["capacity"].Should().Be("4");
        dict["zoneRedundant"].Should().Be("true");
        dict["disableLocalAuth"].Should().Be("false");
        dict["minimumTlsVersion"].Should().Be("1.2");
        dict["autoInflateEnabled"].Should().Be("true");
        dict["maxThroughputUnits"].Should().Be("10");
    }
}
