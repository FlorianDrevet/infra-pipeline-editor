using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.RedisCacheAggregate;

public sealed class RedisCacheTests
{
    private const string DefaultName = "redis-prod";
    private const string DevEnvironment = "dev";
    private const string ProdEnvironment = "prod";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static RedisCache CreateValidRedisCache(bool isExisting = false)
    {
        return RedisCache.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            redisVersion: 6,
            enableNonSslPort: false,
            minimumTlsVersion: new TlsVersion(TlsVersion.Version.Tls12),
            disableAccessKeyAuthentication: false,
            enableAadAuth: true,
            isExisting: isExisting);
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var resourceGroupId = ResourceGroupId.CreateUnique();

        // Act
        var sut = RedisCache.Create(
            resourceGroupId,
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            redisVersion: 6,
            enableNonSslPort: true,
            minimumTlsVersion: new TlsVersion(TlsVersion.Version.Tls12),
            disableAccessKeyAuthentication: true,
            enableAadAuth: true);

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.ResourceGroupId.Should().Be(resourceGroupId);
        sut.Name.Value.Should().Be(DefaultName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.RedisVersion.Should().Be(6);
        sut.EnableNonSslPort.Should().BeTrue();
        sut.MinimumTlsVersion!.Value.Should().Be(TlsVersion.Version.Tls12);
        sut.DisableAccessKeyAuthentication.Should().BeTrue();
        sut.EnableAadAuth.Should().BeTrue();
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_EnvironmentSettings_When_Create_Then_PopulatesCollection()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (RedisCacheSku?)new RedisCacheSku(RedisCacheSku.Sku.Basic), (int?)1, (MaxMemoryPolicy?)new MaxMemoryPolicy(MaxMemoryPolicy.Policy.AllKeysLru)),
            (ProdEnvironment, (RedisCacheSku?)new RedisCacheSku(RedisCacheSku.Sku.Premium), (int?)4, (MaxMemoryPolicy?)null),
        };

        // Act
        var sut = RedisCache.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            redisVersion: 6,
            enableNonSslPort: false,
            minimumTlsVersion: null,
            disableAccessKeyAuthentication: false,
            enableAadAuth: false,
            environmentSettings: settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
    }

    [Fact]
    public void Given_IsExistingTrue_When_Create_Then_IgnoresEnvironmentSettings()
    {
        // Arrange
        var settings = new[]
        {
            (DevEnvironment, (RedisCacheSku?)new RedisCacheSku(RedisCacheSku.Sku.Basic), (int?)1, (MaxMemoryPolicy?)null),
        };

        // Act
        var sut = RedisCache.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultName),
            new Location(DefaultLocationValue),
            redisVersion: 6,
            enableNonSslPort: false,
            minimumTlsVersion: null,
            disableAccessKeyAuthentication: false,
            enableAadAuth: false,
            environmentSettings: settings,
            isExisting: true);

        // Assert
        sut.IsExisting.Should().BeTrue();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── Update ─────────────────────────────────────────────────────────────

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAllProperties()
    {
        // Arrange
        var sut = CreateValidRedisCache();

        // Act
        sut.Update(
            new Name("redis-updated"),
            new Location(Location.LocationEnum.NorthEurope),
            redisVersion: 4,
            enableNonSslPort: true,
            minimumTlsVersion: new TlsVersion(TlsVersion.Version.Tls10),
            disableAccessKeyAuthentication: true,
            enableAadAuth: false);

        // Assert
        sut.Name.Value.Should().Be("redis-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.RedisVersion.Should().Be(4);
        sut.EnableNonSslPort.Should().BeTrue();
        sut.MinimumTlsVersion!.Value.Should().Be(TlsVersion.Version.Tls10);
        sut.DisableAccessKeyAuthentication.Should().BeTrue();
        sut.EnableAadAuth.Should().BeFalse();
    }

    [Fact]
    public void Given_IsExisting_When_Update_Then_OnlyAssignsNameAndLocation()
    {
        // Arrange
        var sut = CreateValidRedisCache(isExisting: true);
        var originalVersion = sut.RedisVersion;

        // Act
        sut.Update(
            new Name("redis-updated"),
            new Location(Location.LocationEnum.NorthEurope),
            redisVersion: 4,
            enableNonSslPort: true,
            minimumTlsVersion: new TlsVersion(TlsVersion.Version.Tls10),
            disableAccessKeyAuthentication: true,
            enableAadAuth: false);

        // Assert
        sut.Name.Value.Should().Be("redis-updated");
        sut.Location.Value.Should().Be(Location.LocationEnum.NorthEurope);
        sut.RedisVersion.Should().Be(originalVersion);
    }

    // ─── SetEnvironmentSettings ────────────────────────────────────────────

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidRedisCache();

        // Act
        sut.SetEnvironmentSettings(
            ProdEnvironment,
            new RedisCacheSku(RedisCacheSku.Sku.Premium),
            capacity: 4,
            new MaxMemoryPolicy(MaxMemoryPolicy.Policy.AllKeysLru));

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntry()
    {
        // Arrange
        var sut = CreateValidRedisCache();
        sut.SetEnvironmentSettings(
            ProdEnvironment,
            new RedisCacheSku(RedisCacheSku.Sku.Basic),
            capacity: 1,
            maxMemoryPolicy: null);

        // Act
        sut.SetEnvironmentSettings(
            ProdEnvironment,
            new RedisCacheSku(RedisCacheSku.Sku.Premium),
            capacity: 4,
            new MaxMemoryPolicy(MaxMemoryPolicy.Policy.VolatileLru));

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.Sku!.Value.Should().Be(RedisCacheSku.Sku.Premium);
        entry.Capacity.Should().Be(4);
        entry.MaxMemoryPolicy!.Value.Should().Be(MaxMemoryPolicy.Policy.VolatileLru);
    }

    [Fact]
    public void Given_IsExisting_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidRedisCache(isExisting: true);

        // Act
        sut.SetEnvironmentSettings(
            ProdEnvironment,
            new RedisCacheSku(RedisCacheSku.Sku.Premium),
            capacity: 4,
            new MaxMemoryPolicy(MaxMemoryPolicy.Policy.AllKeysLru));

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    // ─── SetAllEnvironmentSettings ─────────────────────────────────────────

    [Fact]
    public void Given_NewSettings_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidRedisCache();
        sut.SetEnvironmentSettings(
            DevEnvironment,
            new RedisCacheSku(RedisCacheSku.Sku.Basic),
            capacity: 1,
            maxMemoryPolicy: null);

        var newSettings = new[]
        {
            (ProdEnvironment, (RedisCacheSku?)new RedisCacheSku(RedisCacheSku.Sku.Premium), (int?)4, (MaxMemoryPolicy?)null),
        };

        // Act
        sut.SetAllEnvironmentSettings(newSettings);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle(es => es.EnvironmentName == ProdEnvironment);
    }

    [Fact]
    public void Given_IsExisting_When_SetAllEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidRedisCache(isExisting: true);
        var settings = new[]
        {
            (ProdEnvironment, (RedisCacheSku?)new RedisCacheSku(RedisCacheSku.Sku.Premium), (int?)4, (MaxMemoryPolicy?)null),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }
}
