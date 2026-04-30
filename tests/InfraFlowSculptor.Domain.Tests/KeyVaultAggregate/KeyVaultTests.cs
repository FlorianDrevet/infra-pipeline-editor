using FluentAssertions;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.KeyVaultAggregate;

public sealed class KeyVaultTests
{
    private const string DefaultVaultName = "kv-prod-data";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static KeyVault CreateValidKeyVault(bool isExisting = false)
    {
        return KeyVault.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultVaultName),
            new Location(DefaultLocationValue),
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_DefaultArguments_When_Create_Then_AppliesSecureDefaults()
    {
        // Act
        var sut = CreateValidKeyVault();

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Name.Value.Should().Be(DefaultVaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.EnableRbacAuthorization.Should().BeTrue();
        sut.EnabledForDeployment.Should().BeFalse();
        sut.EnabledForDiskEncryption.Should().BeFalse();
        sut.EnabledForTemplateDeployment.Should().BeFalse();
        sut.EnablePurgeProtection.Should().BeTrue();
        sut.EnableSoftDelete.Should().BeTrue();
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PersistsThem()
    {
        // Arrange
        var settings = new[] { ("dev", (Sku?)new Sku(Sku.SkuEnum.Standard)) };

        // Act
        var sut = KeyVault.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultVaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().EnvironmentName.Should().Be("dev");
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_SkipsEnvironmentSettings()
    {
        // Arrange
        var settings = new[] { ("dev", (Sku?)new Sku(Sku.SkuEnum.Standard)) };

        // Act
        var sut = KeyVault.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultVaultName),
            new Location(DefaultLocationValue),
            environmentSettings: settings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NotExisting_When_Update_Then_UpdatesAllProperties()
    {
        // Arrange
        var sut = CreateValidKeyVault();
        var newName = new Name("kv-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        // Act
        sut.Update(newName, newLocation, false, true, true, true, false, false);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.EnableRbacAuthorization.Should().BeFalse();
        sut.EnabledForDeployment.Should().BeTrue();
        sut.EnabledForDiskEncryption.Should().BeTrue();
        sut.EnabledForTemplateDeployment.Should().BeTrue();
        sut.EnablePurgeProtection.Should().BeFalse();
        sut.EnableSoftDelete.Should().BeFalse();
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyNameAndLocationChange()
    {
        // Arrange
        var sut = CreateValidKeyVault(isExisting: true);
        var newName = new Name("kv-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        // Act
        sut.Update(newName, newLocation, false, true, true, true, false, false);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.EnableRbacAuthorization.Should().BeTrue();
        sut.EnablePurgeProtection.Should().BeTrue();
        sut.EnableSoftDelete.Should().BeTrue();
    }

    // ─── Environment settings ───────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidKeyVault();
        var sku = new Sku(Sku.SkuEnum.Premium);

        // Act
        sut.SetEnvironmentSettings("prod", sku);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().EnvironmentName.Should().Be("prod");
        sut.EnvironmentSettings.Single().Sku.Should().Be(sku);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidKeyVault();
        sut.SetEnvironmentSettings("prod", new Sku(Sku.SkuEnum.Standard));
        var newSku = new Sku(Sku.SkuEnum.Premium);

        // Act
        sut.SetEnvironmentSettings("prod", newSku);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().Sku.Should().Be(newSku);
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidKeyVault(isExisting: true);

        // Act
        sut.SetEnvironmentSettings("dev", new Sku(Sku.SkuEnum.Standard));

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidKeyVault();
        sut.SetEnvironmentSettings("dev", new Sku(Sku.SkuEnum.Standard));
        var settings = new[]
        {
            ("staging", (Sku?)new Sku(Sku.SkuEnum.Standard)),
            ("prod", (Sku?)new Sku(Sku.SkuEnum.Premium)),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Should().NotContain(es => es.EnvironmentName == "dev");
    }
}
