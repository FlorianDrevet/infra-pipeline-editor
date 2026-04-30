using FluentAssertions;
using InfraFlowSculptor.Domain.ApplicationInsightsAggregate.Entities;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.ApplicationInsightsAggregate.Entities;

public sealed class ApplicationInsightsEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var appiId = AzureResourceId.CreateUnique();

        // Act
        var sut = ApplicationInsightsEnvironmentSettings.Create(
            appiId, EnvironmentName, 100m, 90, true, true, "LogAnalytics");

        // Assert
        sut.ApplicationInsightsId.Should().Be(appiId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.SamplingPercentage.Should().Be(100m);
        sut.RetentionInDays.Should().Be(90);
        sut.DisableIpMasking.Should().BeTrue();
        sut.DisableLocalAuth.Should().BeTrue();
        sut.IngestionMode.Should().Be("LogAnalytics");
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = ApplicationInsightsEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, 50m, 30, false, false, "ApplicationInsights");

        // Act
        sut.Update(100m, 90, true, true, "LogAnalytics");

        // Assert
        sut.SamplingPercentage.Should().Be(100m);
        sut.RetentionInDays.Should().Be(90);
        sut.DisableIpMasking.Should().BeTrue();
        sut.DisableLocalAuth.Should().BeTrue();
        sut.IngestionMode.Should().Be("LogAnalytics");
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = ApplicationInsightsEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, null, null, null, null, null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = ApplicationInsightsEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(), EnvironmentName, 100m, 90, true, false, "LogAnalytics");

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["samplingPercentage"].Should().Be("100");
        dict["retentionInDays"].Should().Be("90");
        dict["disableIpMasking"].Should().Be("true");
        dict["disableLocalAuth"].Should().Be("false");
        dict["ingestionMode"].Should().Be("LogAnalytics");
    }
}
