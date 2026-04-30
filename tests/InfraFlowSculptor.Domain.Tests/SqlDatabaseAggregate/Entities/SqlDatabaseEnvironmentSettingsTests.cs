using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.Entities;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.SqlDatabaseAggregate.Entities;

public sealed class SqlDatabaseEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var databaseId = AzureResourceId.CreateUnique();
        var sku = new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Premium);

        // Act
        var sut = SqlDatabaseEnvironmentSettings.Create(databaseId, EnvironmentName, sku, 500, true);

        // Assert
        sut.SqlDatabaseId.Should().Be(databaseId);
        sut.Sku.Should().Be(sku);
        sut.MaxSizeGb.Should().Be(500);
        sut.ZoneRedundant.Should().BeTrue();
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = SqlDatabaseEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            sku: null,
            maxSizeGb: null,
            zoneRedundant: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = SqlDatabaseEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Standard),
            100,
            true);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().HaveCount(3);
        dict["sku"].Should().Be("Standard");
        dict["maxSizeGb"].Should().Be("100");
        dict["zoneRedundant"].Should().Be("true");
    }
}
