using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.NetworkSecurityGroupAggregate;

public sealed class NetworkSecurityGroupTests
{
    private const string DefaultName = "nsg-prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static NetworkSecurityGroup CreateValidNsg(bool isExisting = false)
    {
        return NetworkSecurityGroup.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            isExisting: isExisting);
    }

    private static NsgRuleParameters CreateValidRuleParams(
        string name = "AllowHTTP",
        int priority = 100,
        NsgDirection.Direction direction = NsgDirection.Direction.Inbound,
        NsgAccess.Access access = NsgAccess.Access.Allow)
    {
        return new NsgRuleParameters(
            name,
            priority,
            new NsgDirection(direction),
            new NsgAccess(access),
            new NsgProtocol(NsgProtocol.Protocol.Tcp),
            "10.0.0.0/24",
            "10.0.1.0/24",
            "*",
            "80");
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_ValidArguments_When_Create_Then_AllPropertiesInitialized()
    {
        var _sut = CreateValidNsg();

        _sut.Id.Should().NotBeNull();
        _sut.Name.Value.Should().Be(DefaultName);
        _sut.Location.Value.Should().Be(DefaultLocationValue);
        _sut.IsExisting.Should().BeFalse();
        _sut.SecurityRules.Should().BeEmpty();
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IsExistingSet()
    {
        var _sut = CreateValidNsg(isExisting: true);

        _sut.IsExisting.Should().BeTrue();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_ValidNsg_When_Update_Then_NameAndLocationChanged()
    {
        var _sut = CreateValidNsg();
        var newName = new Name("nsg-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        _sut.Update(newName, newLocation);

        _sut.Name.Should().Be(newName);
        _sut.Location.Should().Be(newLocation);
    }

    // ─── AddRule ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_ValidParams_When_AddRule_Then_RuleAdded()
    {
        var _sut = CreateValidNsg();
        var ruleParams = CreateValidRuleParams();

        var rule = _sut.AddRule(ruleParams);

        _sut.SecurityRules.Should().ContainSingle();
        rule.Name.Should().Be("AllowHTTP");
        rule.Priority.Should().Be(100);
        rule.Direction.Value.Should().Be(NsgDirection.Direction.Inbound);
        rule.Access.Value.Should().Be(NsgAccess.Access.Allow);
        rule.Protocol.Value.Should().Be(NsgProtocol.Protocol.Tcp);
        rule.SourceAddressPrefix.Should().Be("10.0.0.0/24");
        rule.DestinationAddressPrefix.Should().Be("10.0.1.0/24");
        rule.SourcePortRange.Should().Be("*");
        rule.DestinationPortRange.Should().Be("80");
    }

    [Fact]
    public void Given_DuplicateName_When_AddRule_Then_ThrowsInvalidOperation()
    {
        var _sut = CreateValidNsg();
        _sut.AddRule(CreateValidRuleParams(name: "AllowHTTP", priority: 100));

        var act = () => _sut.AddRule(CreateValidRuleParams(name: "AllowHTTP", priority: 200));

        act.Should().Throw<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public void Given_DuplicatePriorityAndDirection_When_AddRule_Then_ThrowsInvalidOperation()
    {
        var _sut = CreateValidNsg();
        _sut.AddRule(CreateValidRuleParams(name: "Rule1", priority: 100, direction: NsgDirection.Direction.Inbound));

        var act = () => _sut.AddRule(CreateValidRuleParams(name: "Rule2", priority: 100, direction: NsgDirection.Direction.Inbound));

        act.Should().Throw<InvalidOperationException>().WithMessage("*priority*");
    }

    [Fact]
    public void Given_SamePriorityDifferentDirection_When_AddRule_Then_Succeeds()
    {
        var _sut = CreateValidNsg();
        _sut.AddRule(CreateValidRuleParams(name: "Rule1", priority: 100, direction: NsgDirection.Direction.Inbound));

        var rule = _sut.AddRule(CreateValidRuleParams(name: "Rule2", priority: 100, direction: NsgDirection.Direction.Outbound));

        _sut.SecurityRules.Should().HaveCount(2);
        rule.Direction.Value.Should().Be(NsgDirection.Direction.Outbound);
    }

    [Fact]
    public void Given_NullName_When_AddRule_Then_ThrowsArgumentException()
    {
        var _sut = CreateValidNsg();

        var act = () => _sut.AddRule(CreateValidRuleParams(name: null!));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Given_EmptyName_When_AddRule_Then_ThrowsArgumentException()
    {
        var _sut = CreateValidNsg();

        var act = () => _sut.AddRule(CreateValidRuleParams(name: ""));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Given_WhitespaceName_When_AddRule_Then_ThrowsArgumentException()
    {
        var _sut = CreateValidNsg();

        var act = () => _sut.AddRule(CreateValidRuleParams(name: "   "));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Given_PriorityBelow100_When_AddRule_Then_ThrowsArgumentOutOfRange()
    {
        var _sut = CreateValidNsg();

        var act = () => _sut.AddRule(CreateValidRuleParams(priority: 99));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Given_PriorityAbove4096_When_AddRule_Then_ThrowsArgumentOutOfRange()
    {
        var _sut = CreateValidNsg();

        var act = () => _sut.AddRule(CreateValidRuleParams(priority: 4097));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(2000)]
    [InlineData(4096)]
    public void Given_BoundaryPriority_When_AddRule_Then_Succeeds(int priority)
    {
        var _sut = CreateValidNsg();

        var rule = _sut.AddRule(CreateValidRuleParams(priority: priority));

        rule.Priority.Should().Be(priority);
    }

    // ─── RemoveRule ─────────────────────────────────────────────────────────

    [Fact]
    public void Given_ExistingRule_When_RemoveRule_Then_RuleRemoved()
    {
        var _sut = CreateValidNsg();
        var rule = _sut.AddRule(CreateValidRuleParams());

        _sut.RemoveRule(rule.Id);

        _sut.SecurityRules.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownRuleId_When_RemoveRule_Then_ThrowsInvalidOperation()
    {
        var _sut = CreateValidNsg();

        var act = () => _sut.RemoveRule(NsgRuleId.CreateUnique());

        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    // ─── UpdateRule ─────────────────────────────────────────────────────────

    [Fact]
    public void Given_ExistingRule_When_UpdateRule_Then_PropertiesUpdated()
    {
        var _sut = CreateValidNsg();
        var rule = _sut.AddRule(CreateValidRuleParams());
        var updatedParams = new NsgRuleParameters(
            "DenyAll",
            200,
            new NsgDirection(NsgDirection.Direction.Outbound),
            new NsgAccess(NsgAccess.Access.Deny),
            new NsgProtocol(NsgProtocol.Protocol.Udp),
            "0.0.0.0/0",
            "0.0.0.0/0",
            "443",
            "443");

        _sut.UpdateRule(rule.Id, updatedParams);

        var updated = _sut.SecurityRules.Single();
        updated.Name.Should().Be("DenyAll");
        updated.Priority.Should().Be(200);
        updated.Direction.Value.Should().Be(NsgDirection.Direction.Outbound);
        updated.Access.Value.Should().Be(NsgAccess.Access.Deny);
        updated.Protocol.Value.Should().Be(NsgProtocol.Protocol.Udp);
        updated.SourceAddressPrefix.Should().Be("0.0.0.0/0");
        updated.DestinationAddressPrefix.Should().Be("0.0.0.0/0");
        updated.SourcePortRange.Should().Be("443");
        updated.DestinationPortRange.Should().Be("443");
    }

    [Fact]
    public void Given_UnknownRuleId_When_UpdateRule_Then_ThrowsInvalidOperation()
    {
        var _sut = CreateValidNsg();

        var act = () => _sut.UpdateRule(NsgRuleId.CreateUnique(), CreateValidRuleParams());

        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public void Given_InvalidName_When_UpdateRule_Then_ThrowsArgumentException()
    {
        var _sut = CreateValidNsg();
        var rule = _sut.AddRule(CreateValidRuleParams());

        var act = () => _sut.UpdateRule(rule.Id, CreateValidRuleParams(name: ""));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Given_PriorityBelow100_When_UpdateRule_Then_ThrowsArgumentOutOfRange()
    {
        var _sut = CreateValidNsg();
        var rule = _sut.AddRule(CreateValidRuleParams());

        var act = () => _sut.UpdateRule(rule.Id, CreateValidRuleParams(priority: 99));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Given_PriorityAbove4096_When_UpdateRule_Then_ThrowsArgumentOutOfRange()
    {
        var _sut = CreateValidNsg();
        var rule = _sut.AddRule(CreateValidRuleParams());

        var act = () => _sut.UpdateRule(rule.Id, CreateValidRuleParams(priority: 4097));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
