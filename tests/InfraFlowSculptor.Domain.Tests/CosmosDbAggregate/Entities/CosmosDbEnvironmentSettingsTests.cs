using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.CosmosDbAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.CosmosDbAggregate.Entities;

public sealed class CosmosDbEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var cosmosId = AzureResourceId.CreateUnique();

        // Act
        var sut = CosmosDbEnvironmentSettings.Create(
            cosmosId,
            EnvironmentName,
            databaseApiType: "SQL",
            consistencyLevel: "BoundedStaleness",
            maxStalenessPrefix: 100,
            maxIntervalInSeconds: 5,
            enableAutomaticFailover: true,
            enableMultipleWriteLocations: false,
            backupPolicyType: "Periodic",
            enableFreeTier: true);

        // Assert
        sut.CosmosDbId.Should().Be(cosmosId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.DatabaseApiType.Should().Be("SQL");
        sut.ConsistencyLevel.Should().Be("BoundedStaleness");
        sut.MaxStalenessPrefix.Should().Be(100);
        sut.MaxIntervalInSeconds.Should().Be(5);
        sut.EnableAutomaticFailover.Should().BeTrue();
        sut.EnableMultipleWriteLocations.Should().BeFalse();
        sut.BackupPolicyType.Should().Be("Periodic");
        sut.EnableFreeTier.Should().BeTrue();
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = CosmosDbEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            databaseApiType: "SQL", consistencyLevel: "Session",
            maxStalenessPrefix: null, maxIntervalInSeconds: null,
            enableAutomaticFailover: false, enableMultipleWriteLocations: false,
            backupPolicyType: "Periodic", enableFreeTier: false);

        // Act
        sut.Update(
            databaseApiType: "MongoDB", consistencyLevel: "Strong",
            maxStalenessPrefix: 200, maxIntervalInSeconds: 10,
            enableAutomaticFailover: true, enableMultipleWriteLocations: true,
            backupPolicyType: "Continuous", enableFreeTier: true);

        // Assert
        sut.DatabaseApiType.Should().Be("MongoDB");
        sut.ConsistencyLevel.Should().Be("Strong");
        sut.MaxStalenessPrefix.Should().Be(200);
        sut.MaxIntervalInSeconds.Should().Be(10);
        sut.EnableAutomaticFailover.Should().BeTrue();
        sut.EnableMultipleWriteLocations.Should().BeTrue();
        sut.BackupPolicyType.Should().Be("Continuous");
        sut.EnableFreeTier.Should().BeTrue();
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = CosmosDbEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            databaseApiType: null, consistencyLevel: null,
            maxStalenessPrefix: null, maxIntervalInSeconds: null,
            enableAutomaticFailover: null, enableMultipleWriteLocations: null,
            backupPolicyType: null, enableFreeTier: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = CosmosDbEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName,
            databaseApiType: "SQL", consistencyLevel: "BoundedStaleness",
            maxStalenessPrefix: 100, maxIntervalInSeconds: 5,
            enableAutomaticFailover: true, enableMultipleWriteLocations: false,
            backupPolicyType: "Periodic", enableFreeTier: true);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["databaseApiType"].Should().Be("SQL");
        dict["consistencyLevel"].Should().Be("BoundedStaleness");
        dict["maxStalenessPrefix"].Should().Be("100");
        dict["maxIntervalInSeconds"].Should().Be("5");
        dict["enableAutomaticFailover"].Should().Be("true");
        dict["enableMultipleWriteLocations"].Should().Be("false");
        dict["backupPolicyType"].Should().Be("Periodic");
        dict["enableFreeTier"].Should().Be("true");
    }
}
