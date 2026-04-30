using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate.Entities;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.KeyVaultAggregate.Entities;

public sealed class KeyVaultEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var keyVaultId = AzureResourceId.CreateUnique();
        var sku = new Sku(Sku.SkuEnum.Premium);

        // Act
        var sut = KeyVaultEnvironmentSettings.Create(keyVaultId, EnvironmentName, sku);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.KeyVaultId.Should().Be(keyVaultId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku.Should().Be(sku);
    }

    [Fact]
    public void Given_NewSku_When_Update_Then_ReplacesSku()
    {
        // Arrange
        var sut = KeyVaultEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new Sku(Sku.SkuEnum.Standard));
        var newSku = new Sku(Sku.SkuEnum.Premium);

        // Act
        sut.Update(newSku);

        // Assert
        sut.Sku.Should().Be(newSku);
    }

    [Fact]
    public void Given_NullSku_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = KeyVaultEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            sku: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_SkuPresent_When_ToDictionary_Then_ContainsSkuKeyLowercase()
    {
        // Arrange
        var sut = KeyVaultEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new Sku(Sku.SkuEnum.Premium));

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().ContainKey("sku").WhoseValue.Should().Be("premium");
    }
}
