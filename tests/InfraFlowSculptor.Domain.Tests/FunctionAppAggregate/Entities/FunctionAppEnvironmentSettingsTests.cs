using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate.Entities;

namespace InfraFlowSculptor.Domain.Tests.FunctionAppAggregate.Entities;

public sealed class FunctionAppEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var functionAppId = AzureResourceId.CreateUnique();

        // Act
        var sut = FunctionAppEnvironmentSettings.Create(
            functionAppId,
            EnvironmentName,
            httpsOnly: true,
            maxInstanceCount: 5,
            dockerImageTag: "v1");

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Id.Value.Should().NotBe(Guid.Empty);
        sut.FunctionAppId.Should().Be(functionAppId);
        sut.EnvironmentName.Should().Be(EnvironmentName);
        sut.HttpsOnly.Should().BeTrue();
        sut.MaxInstanceCount.Should().Be(5);
        sut.DockerImageTag.Should().Be("v1");
    }

    [Fact]
    public void Given_NewValues_When_Update_Then_AssignsAll()
    {
        // Arrange
        var sut = FunctionAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            httpsOnly: false,
            maxInstanceCount: 1,
            dockerImageTag: null);

        // Act
        sut.Update(httpsOnly: true, maxInstanceCount: 20, dockerImageTag: "latest");

        // Assert
        sut.HttpsOnly.Should().BeTrue();
        sut.MaxInstanceCount.Should().Be(20);
        sut.DockerImageTag.Should().Be("latest");
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = FunctionAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            httpsOnly: null,
            maxInstanceCount: null,
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
        var sut = FunctionAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            httpsOnly: true,
            maxInstanceCount: 7,
            dockerImageTag: "v3");

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["httpsOnly"].Should().Be("true");
        dict["maxInstanceCount"].Should().Be("7");
        dict["dockerImageTag"].Should().Be("v3");
    }

    [Fact]
    public void Given_FalseHttpsOnly_When_ToDictionary_Then_ReturnsLowercaseFalse()
    {
        // Arrange
        var sut = FunctionAppEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            httpsOnly: false,
            maxInstanceCount: null,
            dockerImageTag: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["httpsOnly"].Should().Be("false");
    }
}
