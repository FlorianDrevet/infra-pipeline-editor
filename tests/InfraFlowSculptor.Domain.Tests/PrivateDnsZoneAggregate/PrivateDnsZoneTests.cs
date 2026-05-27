using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate;
using InfraFlowSculptor.Domain.PrivateDnsZoneAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.PrivateDnsZoneAggregate;

public sealed class PrivateDnsZoneTests
{
    private const string DefaultName = "privatelink.blob.core.windows.net";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static PrivateDnsZone CreateValidDnsZone(bool isExisting = false)
    {
        return PrivateDnsZone.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_ValidArguments_When_Create_Then_AllPropertiesInitialized()
    {
        var _sut = CreateValidDnsZone();

        _sut.Id.Should().NotBeNull();
        _sut.Name.Value.Should().Be(DefaultName);
        _sut.Location.Value.Should().Be(DefaultLocationValue);
        _sut.IsExisting.Should().BeFalse();
        _sut.VirtualNetworkLinks.Should().BeEmpty();
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IsExistingSet()
    {
        var _sut = CreateValidDnsZone(isExisting: true);

        _sut.IsExisting.Should().BeTrue();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_ValidDnsZone_When_Update_Then_NameAndLocationChanged()
    {
        var _sut = CreateValidDnsZone();
        var newName = new Name("privatelink.database.windows.net");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        _sut.Update(newName, newLocation);

        _sut.Name.Should().Be(newName);
        _sut.Location.Should().Be(newLocation);
    }

    // ─── AddVirtualNetworkLink ──────────────────────────────────────────────

    [Fact]
    public void Given_ValidVnetId_When_AddVirtualNetworkLink_Then_LinkAdded()
    {
        var _sut = CreateValidDnsZone();
        var vnetId = AzureResourceId.CreateUnique();

        var link = _sut.AddVirtualNetworkLink(vnetId, true);

        _sut.VirtualNetworkLinks.Should().ContainSingle();
        link.VirtualNetworkId.Should().Be(vnetId);
        link.EnableAutoRegistration.Should().BeTrue();
        link.PrivateDnsZoneId.Should().Be(_sut.Id);
    }

    [Fact]
    public void Given_AutoRegistrationFalse_When_AddVirtualNetworkLink_Then_AutoRegistrationDisabled()
    {
        var _sut = CreateValidDnsZone();

        var link = _sut.AddVirtualNetworkLink(AzureResourceId.CreateUnique(), false);

        link.EnableAutoRegistration.Should().BeFalse();
    }

    [Fact]
    public void Given_DuplicateVnetId_When_AddVirtualNetworkLink_Then_ThrowsInvalidOperation()
    {
        var _sut = CreateValidDnsZone();
        var vnetId = AzureResourceId.CreateUnique();
        _sut.AddVirtualNetworkLink(vnetId, false);

        var act = () => _sut.AddVirtualNetworkLink(vnetId, true);

        act.Should().Throw<InvalidOperationException>().WithMessage("*already linked*");
    }

    [Fact]
    public void Given_MultipleDistinctVnets_When_AddVirtualNetworkLink_Then_AllAdded()
    {
        var _sut = CreateValidDnsZone();

        _sut.AddVirtualNetworkLink(AzureResourceId.CreateUnique(), true);
        _sut.AddVirtualNetworkLink(AzureResourceId.CreateUnique(), false);

        _sut.VirtualNetworkLinks.Should().HaveCount(2);
    }

    // ─── RemoveVirtualNetworkLink ───────────────────────────────────────────

    [Fact]
    public void Given_ExistingLink_When_RemoveVirtualNetworkLink_Then_LinkRemoved()
    {
        var _sut = CreateValidDnsZone();
        var link = _sut.AddVirtualNetworkLink(AzureResourceId.CreateUnique(), false);

        _sut.RemoveVirtualNetworkLink(link.Id);

        _sut.VirtualNetworkLinks.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownLinkId_When_RemoveVirtualNetworkLink_Then_ThrowsInvalidOperation()
    {
        var _sut = CreateValidDnsZone();

        var act = () => _sut.RemoveVirtualNetworkLink(VirtualNetworkLinkId.CreateUnique());

        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public void Given_TwoLinks_When_RemoveOne_Then_OnlyOneRemains()
    {
        var _sut = CreateValidDnsZone();
        var link1 = _sut.AddVirtualNetworkLink(AzureResourceId.CreateUnique(), true);
        var link2 = _sut.AddVirtualNetworkLink(AzureResourceId.CreateUnique(), false);

        _sut.RemoveVirtualNetworkLink(link1.Id);

        _sut.VirtualNetworkLinks.Should().ContainSingle();
        _sut.VirtualNetworkLinks.Single().Id.Should().Be(link2.Id);
    }
}
