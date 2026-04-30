using FluentAssertions;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.Entities;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.AppServicePlanAggregate.Entities;

public sealed class AppServicePlanEnvironmentSettingsTests
{
    private const string EnvironmentName = "prod";

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Arrange
        var planId = AzureResourceId.CreateUnique();
        var sku = new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.P1v3);

        // Act
        var sut = AppServicePlanEnvironmentSettings.Create(planId, EnvironmentName, sku, 4);

        // Assert
        sut.AppServicePlanId.Should().Be(planId);
        sut.Sku.Should().Be(sku);
        sut.Capacity.Should().Be(4);
    }

    [Fact]
    public void Given_AllNullOverrides_When_ToDictionary_Then_ReturnsEmptyDictionary()
    {
        // Arrange
        var sut = AppServicePlanEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            sku: null,
            capacity: null);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    [Fact]
    public void Given_AllOverrides_When_ToDictionary_Then_ReturnsAllKeys()
    {
        // Arrange
        var sut = AppServicePlanEnvironmentSettings.Create(
            AzureResourceId.CreateUnique(),
            EnvironmentName,
            new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.S2),
            3);

        // Act
        var dict = sut.ToDictionary();

        // Assert
        dict["sku"].Should().Be("S2");
        dict["capacity"].Should().Be("3");
    }
}
