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
}
