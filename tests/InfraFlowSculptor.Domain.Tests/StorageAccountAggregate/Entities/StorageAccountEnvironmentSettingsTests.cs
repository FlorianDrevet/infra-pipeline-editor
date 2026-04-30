using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate.Entities;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.StorageAccountAggregate.Entities;

public sealed class StorageAccountEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var storageAccountId = AzureResourceId.CreateUnique();
        var sku = new StorageAccountSku(StorageAccountSku.Sku.Standard_LRS);

        // Act
        var sut = StorageAccountEnvironmentSettings.Create(storageAccountId, EnvironmentName, sku);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.StorageAccountId.Should().Be(storageAccountId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku.Should().Be(sku);
    }

    [Fact]
    public void Given_NewSku_When_Update_Then_ReplacesSku()
    {
        // Arrange
        var sut = StorageAccountEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new StorageAccountSku(StorageAccountSku.Sku.Standard_LRS));
        var newSku = new StorageAccountSku(StorageAccountSku.Sku.Premium_LRS);

        // Act
        sut.Update(newSku);

        // Assert
        sut.Sku.Should().Be(newSku);
    }

    [Fact]
    public void Given_NullSku_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = StorageAccountEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            sku: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_SkuPresent_When_ToDictionary_Then_ContainsSkuKey()
    {
        // Arrange
        var sut = StorageAccountEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new StorageAccountSku(StorageAccountSku.Sku.Standard_GRS));

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().ContainKey("sku").WhoseValue.Should().Be(StorageAccountSku.Sku.Standard_GRS.ToString());
    }
}
