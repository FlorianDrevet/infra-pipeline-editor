using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate.Entities;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.RedisCacheAggregate.Entities;

public sealed class RedisCacheEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var redisCacheId = AzureResourceId.CreateUnique();

        // Act
        var sut = RedisCacheEnvironmentSettings.Create(
            redisCacheId,
            EnvironmentName,
            new RedisCacheSku(RedisCacheSku.Sku.Premium),
            capacity: 4,
            new MaxMemoryPolicy(MaxMemoryPolicy.Policy.AllKeysLru));

        // Assert
        sut.RedisCacheId.Should().Be(redisCacheId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku!.Value.Should().Be(RedisCacheSku.Sku.Premium);
        sut.Capacity.Should().Be(4);
        sut.MaxMemoryPolicy!.Value.Should().Be(MaxMemoryPolicy.Policy.AllKeysLru);
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = RedisCacheEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new RedisCacheSku(RedisCacheSku.Sku.Basic),
            capacity: 1,
            maxMemoryPolicy: null);

        // Act
        sut.Update(
            new RedisCacheSku(RedisCacheSku.Sku.Premium),
            capacity: 4,
            new MaxMemoryPolicy(MaxMemoryPolicy.Policy.VolatileLru));

        // Assert
        sut.Sku!.Value.Should().Be(RedisCacheSku.Sku.Premium);
        sut.Capacity.Should().Be(4);
        sut.MaxMemoryPolicy!.Value.Should().Be(MaxMemoryPolicy.Policy.VolatileLru);
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = RedisCacheEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            sku: null,
            capacity: null,
            maxMemoryPolicy: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_PremiumSku_When_ToDictionary_Then_SkuFamilyIsP()
    {
        // Arrange
        var sut = RedisCacheEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new RedisCacheSku(RedisCacheSku.Sku.Premium),
            capacity: 4,
            maxMemoryPolicy: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["skuName"].Should().Be("Premium");
        dict["skuFamily"].Should().Be("P");
        dict["capacity"].Should().Be("4");
    }

    [Fact]
    public void Given_StandardSku_When_ToDictionary_Then_SkuFamilyIsC()
    {
        // Arrange
        var sut = RedisCacheEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new RedisCacheSku(RedisCacheSku.Sku.Standard),
            capacity: 2,
            maxMemoryPolicy: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["skuName"].Should().Be("Standard");
        dict["skuFamily"].Should().Be("C");
    }
}
