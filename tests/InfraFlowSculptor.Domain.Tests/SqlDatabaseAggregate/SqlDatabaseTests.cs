using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.SqlDatabaseAggregate;

public sealed class SqlDatabaseTests
{
    private const string DefaultDatabaseName = "appdb";
    private const string DefaultCollation = "SQL_Latin1_General_CP1_CI_AS";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static SqlDatabase CreateValidSqlDatabase(bool isExisting = false)
    {
        return SqlDatabase.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultDatabaseName),
            new Location(DefaultLocationValue),
            AzureResourceId.CreateUnique(),
            DefaultCollation,
            isExisting: isExisting);
    }

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Act
        var sut = CreateValidSqlDatabase();

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Name.Value.Should().Be(DefaultDatabaseName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.Collation.Should().Be(DefaultCollation);
        sut.SqlServerId.Should().NotBeNull();
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NotExisting_When_Update_Then_UpdatesAllProperties()
    {
        // Arrange
        var sut = CreateValidSqlDatabase();
        var newServerId = AzureResourceId.CreateUnique();
        var newName = new Name("newdb");
        var newLocation = new Location(Location.LocationEnum.EastUS);
        const string newCollation = "Latin1_General_100_CI_AS_SC_UTF8";

        // Act
        sut.Update(newName, newLocation, newServerId, newCollation);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.SqlServerId.Should().Be(newServerId);
        sut.Collation.Should().Be(newCollation);
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyNameAndLocationChange()
    {
        // Arrange
        var sut = CreateValidSqlDatabase(isExisting: true);
        var initialServerId = sut.SqlServerId;
        var newName = new Name("renameddb");
        var newLocation = new Location(Location.LocationEnum.EastUS);

        // Act
        sut.Update(newName, newLocation, AzureResourceId.CreateUnique(), "OtherCollation");

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.SqlServerId.Should().Be(initialServerId);
        sut.Collation.Should().Be(DefaultCollation);
    }

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidSqlDatabase();
        var sku = new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Standard);

        // Act
        sut.SetEnvironmentSettings("prod", sku, 250, true);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.Sku.Should().Be(sku);
        entry.MaxSizeGb.Should().Be(250);
        entry.ZoneRedundant.Should().BeTrue();
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidSqlDatabase();
        sut.SetEnvironmentSettings("prod", new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Basic), 1, false);
        var newSku = new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Premium);

        // Act
        sut.SetEnvironmentSettings("prod", newSku, 500, true);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().Sku.Should().Be(newSku);
        sut.EnvironmentSettings.Single().MaxSizeGb.Should().Be(500);
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidSqlDatabase(isExisting: true);

        // Act
        sut.SetEnvironmentSettings("prod", new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Standard), 100, false);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidSqlDatabase();
        sut.SetEnvironmentSettings("dev", new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Basic), 1, false);
        var settings = new[]
        {
            ("staging", (SqlDatabaseSku?)new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Standard), (int?)50, (bool?)false),
            ("prod", (SqlDatabaseSku?)new SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum.Premium), (int?)500, (bool?)true),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Should().NotContain(es => es.EnvironmentName == "dev");
    }
}
