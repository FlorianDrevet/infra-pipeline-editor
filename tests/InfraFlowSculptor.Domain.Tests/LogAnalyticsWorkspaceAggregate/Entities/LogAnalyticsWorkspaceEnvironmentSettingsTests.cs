using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.LogAnalyticsWorkspaceAggregate.Entities;

public sealed class LogAnalyticsWorkspaceEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var lawId = AzureResourceId.CreateUnique();

        // Act
        var sut = LogAnalyticsWorkspaceEnvironmentSettings.Create(
            lawId, EnvironmentName, "PerGB2018", 90, 10m);

        // Assert
        sut.LogAnalyticsWorkspaceId.Should().Be(lawId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.Sku.Should().Be("PerGB2018");
        sut.RetentionInDays.Should().Be(90);
        sut.DailyQuotaGb.Should().Be(10m);
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = LogAnalyticsWorkspaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, "Free", 30, 1m);

        // Act
        sut.Update("PerGB2018", 90, 10m);

        // Assert
        sut.Sku.Should().Be("PerGB2018");
        sut.RetentionInDays.Should().Be(90);
        sut.DailyQuotaGb.Should().Be(10m);
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = LogAnalyticsWorkspaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, null, null, null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = LogAnalyticsWorkspaceEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, "PerGB2018", 90, 10m);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["sku"].Should().Be("PerGB2018");
        dict["retentionInDays"].Should().Be("90");
        dict["dailyQuotaGb"].Should().Be("10");
    }
}
