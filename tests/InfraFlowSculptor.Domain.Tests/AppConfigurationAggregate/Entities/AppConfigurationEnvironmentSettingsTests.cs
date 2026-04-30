using FluentAssertions;
using InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.AppConfigurationAggregate.Entities;

public sealed class AppConfigurationEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var appCsId = AzureResourceId.CreateUnique();

        // Act
        var sut = AppConfigurationEnvironmentSettings.Create(
            appCsId, EnvironmentName, "Standard", 30, true, true, "Disabled");

        // Assert
        sut.AppConfigurationId.Should().Be(appCsId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku.Should().Be("Standard");
        sut.SoftDeleteRetentionInDays.Should().Be(30);
        sut.PurgeProtectionEnabled.Should().BeTrue();
        sut.DisableLocalAuth.Should().BeTrue();
        sut.PublicNetworkAccess.Should().Be("Disabled");
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = AppConfigurationEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, "Free", 7, false, false, "Enabled");

        // Act
        sut.Update("Standard", 30, true, true, "Disabled");

        // Assert
        sut.Sku.Should().Be("Standard");
        sut.SoftDeleteRetentionInDays.Should().Be(30);
        sut.PurgeProtectionEnabled.Should().BeTrue();
        sut.DisableLocalAuth.Should().BeTrue();
        sut.PublicNetworkAccess.Should().Be("Disabled");
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = AppConfigurationEnvironmentSettings.Create(
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
        var sut = AppConfigurationEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, "Standard", 30, true, false, "Disabled");

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["sku"].Should().Be("Standard");
        dict["softDeleteRetentionInDays"].Should().Be("30");
        dict["enablePurgeProtection"].Should().Be("true");
        dict["disableLocalAuth"].Should().Be("false");
        dict["publicNetworkAccess"].Should().Be("Disabled");
    }
}
