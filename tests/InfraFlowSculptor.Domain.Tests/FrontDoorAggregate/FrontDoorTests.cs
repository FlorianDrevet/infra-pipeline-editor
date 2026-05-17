using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FrontDoorAggregate;
using InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.FrontDoorAggregate;

public sealed class FrontDoorTests
{
    private const string DefaultName = "fd-prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static FrontDoor CreateValidFrontDoor(bool isExisting = false, bool wafPolicyEnabled = false)
    {
        return FrontDoor.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            wafPolicyEnabled: wafPolicyEnabled,
            isExisting: isExisting);
    }

    private static (string EnvironmentName, FrontDoorSku Sku)[] CreateDefaultSettings()
    {
        return [("dev", new FrontDoorSku(FrontDoorSku.Sku.StandardAzureFrontDoor))];
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_ValidArguments_When_Create_Then_AllPropertiesInitialized()
    {
        var _sut = CreateValidFrontDoor();

        _sut.Id.Should().NotBeNull();
        _sut.Name.Value.Should().Be(DefaultName);
        _sut.Location.Value.Should().Be(DefaultLocationValue);
        _sut.WafPolicyEnabled.Should().BeFalse();
        _sut.IsExisting.Should().BeFalse();
        _sut.Origins.Should().BeEmpty();
        _sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_WafEnabled_When_Create_Then_WafPolicyEnabledIsTrue()
    {
        var _sut = CreateValidFrontDoor(wafPolicyEnabled: true);

        _sut.WafPolicyEnabled.Should().BeTrue();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_SettingsApplied()
    {
        var settings = CreateDefaultSettings();

        var _sut = FrontDoor.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings);

        _sut.EnvironmentSettings.Should().ContainSingle();
        _sut.EnvironmentSettings.Single().EnvironmentName.Should().Be("dev");
        _sut.EnvironmentSettings.Single().Sku.Value.Should().Be(FrontDoorSku.Sku.StandardAzureFrontDoor);
    }

    [Fact]
    public void Given_IsExistingTrueWithSettings_When_Create_Then_SettingsNotApplied()
    {
        var settings = CreateDefaultSettings();

        var _sut = FrontDoor.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings,
            isExisting: true);

        _sut.IsExisting.Should().BeTrue();
        _sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NullEnvironmentSettings_When_Create_Then_SettingsEmpty()
    {
        var _sut = FrontDoor.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            environmentSettings: null);

        _sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NotExisting_When_Update_Then_NameLocationAndWafChanged()
    {
        var _sut = CreateValidFrontDoor();
        var newName = new Name("fd-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        _sut.Update(newName, newLocation, true);

        _sut.Name.Should().Be(newName);
        _sut.Location.Should().Be(newLocation);
        _sut.WafPolicyEnabled.Should().BeTrue();
    }

    [Fact]
    public void Given_IsExisting_When_Update_Then_WafNotChanged()
    {
        var _sut = CreateValidFrontDoor(isExisting: true, wafPolicyEnabled: false);
        var newName = new Name("fd-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        _sut.Update(newName, newLocation, true);

        _sut.Name.Should().Be(newName);
        _sut.Location.Should().Be(newLocation);
        _sut.WafPolicyEnabled.Should().BeFalse();
    }

    // ─── AddOrigin ──────────────────────────────────────────────────────────

    [Fact]
    public void Given_ValidParams_When_AddOrigin_Then_OriginAdded()
    {
        var _sut = CreateValidFrontDoor();
        var targetId = AzureResourceId.CreateUnique();

        var origin = _sut.AddOrigin(targetId, "app.example.com", true, 500, 3);

        _sut.Origins.Should().ContainSingle();
        origin.TargetResourceId.Should().Be(targetId);
        origin.HostName.Should().Be("app.example.com");
        origin.PrivateLinkEnabled.Should().BeTrue();
        origin.Weight.Should().Be(500);
        origin.Priority.Should().Be(3);
    }

    [Fact]
    public void Given_NullHostName_When_AddOrigin_Then_HostNameIsNull()
    {
        var _sut = CreateValidFrontDoor();

        var origin = _sut.AddOrigin(AzureResourceId.CreateUnique(), null, false, 1, 1);

        origin.HostName.Should().BeNull();
    }

    [Fact]
    public void Given_WeightBelowOne_When_AddOrigin_Then_ThrowsArgumentOutOfRange()
    {
        var _sut = CreateValidFrontDoor();

        var act = () => _sut.AddOrigin(AzureResourceId.CreateUnique(), null, false, 0, 1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Given_WeightAbove1000_When_AddOrigin_Then_ThrowsArgumentOutOfRange()
    {
        var _sut = CreateValidFrontDoor();

        var act = () => _sut.AddOrigin(AzureResourceId.CreateUnique(), null, false, 1001, 1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Given_PriorityBelowOne_When_AddOrigin_Then_ThrowsArgumentOutOfRange()
    {
        var _sut = CreateValidFrontDoor();

        var act = () => _sut.AddOrigin(AzureResourceId.CreateUnique(), null, false, 1, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Given_PriorityAboveFive_When_AddOrigin_Then_ThrowsArgumentOutOfRange()
    {
        var _sut = CreateValidFrontDoor();

        var act = () => _sut.AddOrigin(AzureResourceId.CreateUnique(), null, false, 1, 6);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    [InlineData(1000)]
    public void Given_BoundaryWeight_When_AddOrigin_Then_Succeeds(int weight)
    {
        var _sut = CreateValidFrontDoor();

        var origin = _sut.AddOrigin(AzureResourceId.CreateUnique(), null, false, weight, 1);

        origin.Weight.Should().Be(weight);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Given_BoundaryPriority_When_AddOrigin_Then_Succeeds(int priority)
    {
        var _sut = CreateValidFrontDoor();

        var origin = _sut.AddOrigin(AzureResourceId.CreateUnique(), null, false, 1, priority);

        origin.Priority.Should().Be(priority);
    }

    // ─── UpdateOrigin ───────────────────────────────────────────────────────

    [Fact]
    public void Given_ExistingOrigin_When_UpdateOrigin_Then_PropertiesUpdated()
    {
        var _sut = CreateValidFrontDoor();
        var origin = _sut.AddOrigin(AzureResourceId.CreateUnique(), "old.example.com", false, 100, 1);

        _sut.UpdateOrigin(origin.Id, "new.example.com", true, 200, 2);

        var updated = _sut.Origins.Single();
        updated.HostName.Should().Be("new.example.com");
        updated.PrivateLinkEnabled.Should().BeTrue();
        updated.Weight.Should().Be(200);
        updated.Priority.Should().Be(2);
    }

    [Fact]
    public void Given_UnknownOriginId_When_UpdateOrigin_Then_ThrowsInvalidOperation()
    {
        var _sut = CreateValidFrontDoor();

        var act = () => _sut.UpdateOrigin(FrontDoorOriginId.CreateUnique(), "host", false, 1, 1);

        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    // ─── RemoveOrigin ───────────────────────────────────────────────────────

    [Fact]
    public void Given_ExistingOrigin_When_RemoveOrigin_Then_OriginRemoved()
    {
        var _sut = CreateValidFrontDoor();
        var origin = _sut.AddOrigin(AzureResourceId.CreateUnique(), null, false, 1, 1);

        _sut.RemoveOrigin(origin.Id);

        _sut.Origins.Should().BeEmpty();
    }

    [Fact]
    public void Given_UnknownOriginId_When_RemoveOrigin_Then_ThrowsInvalidOperation()
    {
        var _sut = CreateValidFrontDoor();

        var act = () => _sut.RemoveOrigin(FrontDoorOriginId.CreateUnique());

        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    // ─── SetEnvironmentSettings ─────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_SettingAdded()
    {
        var _sut = CreateValidFrontDoor();
        var sku = new FrontDoorSku(FrontDoorSku.Sku.PremiumAzureFrontDoor);

        _sut.SetEnvironmentSettings("staging", sku);

        _sut.EnvironmentSettings.Should().ContainSingle();
        _sut.EnvironmentSettings.Single().EnvironmentName.Should().Be("staging");
        _sut.EnvironmentSettings.Single().Sku.Value.Should().Be(FrontDoorSku.Sku.PremiumAzureFrontDoor);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_SettingUpdated()
    {
        var _sut = CreateValidFrontDoor();
        _sut.SetEnvironmentSettings("dev", new FrontDoorSku(FrontDoorSku.Sku.StandardAzureFrontDoor));

        _sut.SetEnvironmentSettings("dev", new FrontDoorSku(FrontDoorSku.Sku.PremiumAzureFrontDoor));

        _sut.EnvironmentSettings.Should().ContainSingle();
        _sut.EnvironmentSettings.Single().Sku.Value.Should().Be(FrontDoorSku.Sku.PremiumAzureFrontDoor);
    }

    [Fact]
    public void Given_IsExisting_When_SetEnvironmentSettings_Then_NoOp()
    {
        var _sut = CreateValidFrontDoor(isExisting: true);

        _sut.SetEnvironmentSettings("dev", new FrontDoorSku(FrontDoorSku.Sku.StandardAzureFrontDoor));

        _sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── SetAllEnvironmentSettings ──────────────────────────────────────────

    [Fact]
    public void Given_Settings_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        var _sut = CreateValidFrontDoor();
        _sut.SetEnvironmentSettings("old", new FrontDoorSku(FrontDoorSku.Sku.StandardAzureFrontDoor));

        var newSettings = new (string, FrontDoorSku)[]
        {
            ("prod", new FrontDoorSku(FrontDoorSku.Sku.PremiumAzureFrontDoor)),
            ("staging", new FrontDoorSku(FrontDoorSku.Sku.StandardAzureFrontDoor))
        };

        _sut.SetAllEnvironmentSettings(newSettings);

        _sut.EnvironmentSettings.Should().HaveCount(2);
        _sut.EnvironmentSettings.Select(es => es.EnvironmentName).Should().BeEquivalentTo("prod", "staging");
    }

    [Fact]
    public void Given_IsExisting_When_SetAllEnvironmentSettings_Then_NoOp()
    {
        var _sut = CreateValidFrontDoor(isExisting: true);

        var settings = new (string, FrontDoorSku)[]
        {
            ("prod", new FrontDoorSku(FrontDoorSku.Sku.PremiumAzureFrontDoor))
        };

        _sut.SetAllEnvironmentSettings(settings);

        _sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyList_When_SetAllEnvironmentSettings_Then_ClearsAll()
    {
        var _sut = CreateValidFrontDoor();
        _sut.SetEnvironmentSettings("dev", new FrontDoorSku(FrontDoorSku.Sku.StandardAzureFrontDoor));

        _sut.SetAllEnvironmentSettings([]);

        _sut.EnvironmentSettings.Should().BeEmpty();
    }
}
