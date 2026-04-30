using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.WebAppAggregate.Entities;

public sealed class WebAppEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var webAppId = AzureResourceId.CreateUnique();

        // Act
        var sut = WebAppEnvironmentSettings.Create(webAppId, EnvironmentName, true, false, "v1.0");

        // Assert
        sut.WebAppId.Should().Be(webAppId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.AlwaysOn.Should().BeTrue();
        sut.HttpsOnly.Should().BeFalse();
        sut.DockerImageTag.Should().Be("v1.0");
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = WebAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            alwaysOn: null,
            httpsOnly: null,
            dockerImageTag: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = WebAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            alwaysOn: true,
            httpsOnly: false,
            dockerImageTag: "latest");

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["alwaysOn"].Should().Be("true");
        dict["httpsOnly"].Should().Be("false");
        dict["dockerImageTag"].Should().Be("latest");
    }
}
