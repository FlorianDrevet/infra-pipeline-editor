using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.Entities;
using InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.DocumentIntelligenceAggregate.Entities;

public sealed class DocumentIntelligenceEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var docIntelId = AzureResourceId.CreateUnique();
        var sku = new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.S0);
        var publicNetworkAccess = new PublicNetworkAccessMode(PublicNetworkAccessMode.Mode.Disabled);

        // Act
        var sut = DocumentIntelligenceEnvironmentSettings.Create(
            docIntelId, EnvironmentName, sku, publicNetworkAccess, disableLocalAuth: true);

        // Assert
        sut.DocumentIntelligenceId.Should().Be(docIntelId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku!.Value.Should().Be(DocumentIntelligenceSku.Sku.S0);
        sut.PublicNetworkAccess!.Value.Should().Be(PublicNetworkAccessMode.Mode.Disabled);
        sut.DisableLocalAuth.Should().BeTrue();
    }

    [Fact]
    public void Given_NullOverrides_When_Create_Then_OverridesAreNull()
    {
        // Arrange
        var docIntelId = AzureResourceId.CreateUnique();

        // Act
        var sut = DocumentIntelligenceEnvironmentSettings.Create(
            docIntelId, EnvironmentName, sku: null, publicNetworkAccess: null, disableLocalAuth: false);

        // Assert
        sut.Sku.Should().BeNull();
        sut.PublicNetworkAccess.Should().BeNull();
        sut.DisableLocalAuth.Should().BeFalse();
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = DocumentIntelligenceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.F0),
            new PublicNetworkAccessMode(PublicNetworkAccessMode.Mode.Enabled),
            disableLocalAuth: false);

        // Act
        sut.Update(
            new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.S0),
            new PublicNetworkAccessMode(PublicNetworkAccessMode.Mode.Disabled),
            disableLocalAuth: true);

        // Assert
        sut.Sku!.Value.Should().Be(DocumentIntelligenceSku.Sku.S0);
        sut.PublicNetworkAccess!.Value.Should().Be(PublicNetworkAccessMode.Mode.Disabled);
        sut.DisableLocalAuth.Should().BeTrue();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = DocumentIntelligenceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.S0),
            new PublicNetworkAccessMode(PublicNetworkAccessMode.Mode.Disabled),
            disableLocalAuth: true);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["skuName"].Should().Be("S0");
        dict["publicNetworkAccess"].Should().Be("Disabled");
        dict["disableLocalAuth"].Should().Be("true");
    }

    [Fact]
    public void Given_NoOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = DocumentIntelligenceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            sku: null,
            publicNetworkAccess: null,
            disableLocalAuth: false);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_SkuOnly_When_ToDictionary_Then_ContainsOnlySkuKey()
    {
        // Arrange
        var sut = DocumentIntelligenceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new DocumentIntelligenceSku(DocumentIntelligenceSku.Sku.F0),
            publicNetworkAccess: null,
            disableLocalAuth: false);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().ContainKey("skuName").WhoseValue.Should().Be("F0");
        dict.Should().NotContainKey("publicNetworkAccess");
        dict.Should().NotContainKey("disableLocalAuth");
    }
}
