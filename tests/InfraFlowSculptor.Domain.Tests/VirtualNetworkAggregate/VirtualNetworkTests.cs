using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;

namespace InfraFlowSculptor.Domain.Tests.VirtualNetworkAggregate;

public sealed class VirtualNetworkTests
{
    private const string DefaultVnetName = "vnet-prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static (string EnvironmentName, IReadOnlyList<string> AddressSpaces, IReadOnlyList<string>? DnsServers, bool EnableDdosProtection) Env(
        string name, bool ddos, params string[] addressSpaces)
        => (name, addressSpaces, null, ddos);

    [Fact]
    public void Given_PerEnvironmentSettings_When_Create_Then_PersistsDdosPerEnvironment()
    {
        // Act
        var sut = VirtualNetwork.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultVnetName),
            new Location(DefaultLocationValue),
            environmentSettings:
            [
                Env("dev", ddos: false, "10.1.0.0/16"),
                Env("prod", ddos: true, "10.2.0.0/16"),
            ]);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Single(e => e.EnvironmentName == "dev").EnableDdosProtection.Should().BeFalse();
        sut.EnvironmentSettings.Single(e => e.EnvironmentName == "prod").EnableDdosProtection.Should().BeTrue();
    }

    [Fact]
    public void Given_ExistingVnet_When_SetAllEnvironmentSettings_Then_Ignored()
    {
        // Arrange
        var sut = VirtualNetwork.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultVnetName),
            new Location(DefaultLocationValue),
            isExisting: true);

        // Act
        sut.SetAllEnvironmentSettings([Env("prod", ddos: true, "10.0.0.0/16")]);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NewEnvironmentName_When_SetEnvironmentSettings_Then_AddsEntryWithDdos()
    {
        // Arrange
        var sut = VirtualNetwork.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultVnetName),
            new Location(DefaultLocationValue));

        // Act
        sut.SetEnvironmentSettings("prod", ["10.0.0.0/16"], dnsServers: null, enableDdosProtection: true);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle()
            .Which.EnableDdosProtection.Should().BeTrue();
    }
}
