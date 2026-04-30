using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.SqlServerAggregate.Entities;

public sealed class SqlServerEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";
    private const string TlsVersion = "1.2";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var serverId = AzureResourceId.CreateUnique();

        // Act
        var sut = SqlServerEnvironmentSettings.Create(serverId, EnvironmentName, TlsVersion);

        // Assert
        sut.SqlServerId.Should().Be(serverId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.MinimalTlsVersion.Should().Be(TlsVersion);
    }

    [Fact]
    public void Given_NullTls_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = SqlServerEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            minimalTlsVersion: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_TlsPresent_When_ToDictionary_Then_ContainsTlsKey()
    {
        // Arrange
        var sut = SqlServerEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            TlsVersion);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().ContainKey("minimalTlsVersion").WhoseValue.Should().Be(TlsVersion);
    }
}
