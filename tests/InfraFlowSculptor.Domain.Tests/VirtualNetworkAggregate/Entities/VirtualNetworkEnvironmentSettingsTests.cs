using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.VirtualNetworkAggregate.Entities;

public sealed class VirtualNetworkEnvironmentSettingsTests
{
    // The entity factory is internal (created through the aggregate root), so settings are
    // obtained via VirtualNetwork and the public Update surface is exercised directly.
    private static VirtualNetworkEnvironmentSettings CreateSettings(bool enableDdosProtection)
    {
        IReadOnlyList<(string EnvironmentName, IReadOnlyList<string> AddressSpaces, IReadOnlyList<string>? DnsServers, bool EnableDdosProtection)> envs =
            [("prod", ["10.0.0.0/16"], ["10.0.0.4"], enableDdosProtection)];

        var vnet = VirtualNetwork.Create(
            ResourceGroupId.CreateUnique(),
            new Name("vnet-x"),
            new Location(Location.LocationEnum.WestEurope),
            environmentSettings: envs);

        return vnet.EnvironmentSettings.Single();
    }

    [Fact]
    public void Given_FactoryViaAggregate_When_Created_Then_CarriesDdosFlag()
    {
        // Act
        var sut = CreateSettings(enableDdosProtection: true);

        // Assert
        sut.EnvironmentName.Should().Be("prod");
        sut.AddressSpaces.Should().BeEquivalentTo("10.0.0.0/16");
        sut.DnsServers.Should().BeEquivalentTo("10.0.0.4");
        sut.EnableDdosProtection.Should().BeTrue();
    }

    [Fact]
    public void Given_ExistingSettings_When_Update_Then_OverwritesDdosAndAddresses()
    {
        // Arrange
        var sut = CreateSettings(enableDdosProtection: false);

        // Act
        sut.Update(["172.16.0.0/12"], ["168.63.129.16"], enableDdosProtection: true);

        // Assert
        sut.AddressSpaces.Should().BeEquivalentTo("172.16.0.0/12");
        sut.DnsServers.Should().BeEquivalentTo("168.63.129.16");
        sut.EnableDdosProtection.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenInputListMutatedAfterwards_EntityAddressSpacesUnchanged()
    {
        // Arrange — build a mutable list and pass it to the factory via the aggregate
        var mutableAddressSpaces = new List<string> { "10.0.0.0/16" };
        var mutableDnsServers = new List<string> { "10.0.0.4" };

        IReadOnlyList<(string EnvironmentName, IReadOnlyList<string> AddressSpaces, IReadOnlyList<string>? DnsServers, bool EnableDdosProtection)> envs =
            [("dev", mutableAddressSpaces, mutableDnsServers, false)];

        var vnet = VirtualNetwork.Create(
            ResourceGroupId.CreateUnique(),
            new Name("vnet-isolation-test"),
            new Location(Location.LocationEnum.WestEurope),
            environmentSettings: envs);

        var settings = vnet.EnvironmentSettings.Single();

        // Act — mutate the original lists after creation
        mutableAddressSpaces.Add("192.168.0.0/24");
        mutableDnsServers.Add("8.8.8.8");

        // Assert — entity data must be unchanged
        settings.AddressSpaces.Should().BeEquivalentTo(["10.0.0.0/16"],
            because: "Create must take a defensive copy of the address spaces list");
        settings.DnsServers.Should().BeEquivalentTo(["10.0.0.4"],
            because: "Create must take a defensive copy of the DNS servers list");
    }

    [Fact]
    public void Update_WhenInputListMutatedAfterwards_EntityAddressSpacesUnchanged()
    {
        // Arrange
        var sut = CreateSettings(enableDdosProtection: false);
        var mutableAddressSpaces = new List<string> { "172.16.0.0/12" };
        var mutableDnsServers = new List<string> { "168.63.129.16" };

        sut.Update(mutableAddressSpaces, mutableDnsServers, enableDdosProtection: false);

        // Act — mutate the original lists after update
        mutableAddressSpaces.Add("10.99.0.0/24");
        mutableDnsServers.Add("1.1.1.1");

        // Assert — entity data must be unchanged
        sut.AddressSpaces.Should().BeEquivalentTo(["172.16.0.0/12"],
            because: "Update must take a defensive copy of the address spaces list");
        sut.DnsServers.Should().BeEquivalentTo(["168.63.129.16"],
            because: "Update must take a defensive copy of the DNS servers list");
    }
}
