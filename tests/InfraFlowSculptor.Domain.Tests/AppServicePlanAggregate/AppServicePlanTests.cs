using FluentAssertions;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Tests.AppServicePlanAggregate;

public sealed class AppServicePlanTests
{
    private const string DefaultPlanName = "asp-prod-shared";
    private const Location.LocationEnum DefaultLocationValue = Location.LocationEnum.WestEurope;

    private static AppServicePlan CreateValidAppServicePlan(bool isExisting = false)
    {
        return AppServicePlan.Create(
            ResourceGroupId.CreateUnique(),
            new Name(DefaultPlanName),
            new Location(DefaultLocationValue),
            new AppServicePlanOsType(AppServicePlanOsType.AppServicePlanOsTypeEnum.Linux),
            isExisting: isExisting);
    }

    [Fact]
    public void Given_FactoryArguments_When_Create_Then_InitializesProperties()
    {
        // Act
        var sut = CreateValidAppServicePlan();

        // Assert
        sut.Id.Should().NotBeNull();
        sut.Name.Value.Should().Be(DefaultPlanName);
        sut.Location.Value.Should().Be(DefaultLocationValue);
        sut.OsType.Value.Should().Be(AppServicePlanOsType.AppServicePlanOsTypeEnum.Linux);
        sut.IsExisting.Should().BeFalse();
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_NotExisting_When_Update_Then_UpdatesAllProperties()
    {
        // Arrange
        var sut = CreateValidAppServicePlan();
        var newName = new Name("asp-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);
        var newOsType = new AppServicePlanOsType(AppServicePlanOsType.AppServicePlanOsTypeEnum.Windows);

        // Act
        sut.Update(newName, newLocation, newOsType);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.OsType.Should().Be(newOsType);
    }

    [Fact]
    public void Given_IsExistingResource_When_Update_Then_OnlyNameAndLocationChange()
    {
        // Arrange
        var sut = CreateValidAppServicePlan(isExisting: true);
        var newName = new Name("asp-renamed");
        var newLocation = new Location(Location.LocationEnum.EastUS);
        var newOsType = new AppServicePlanOsType(AppServicePlanOsType.AppServicePlanOsTypeEnum.Windows);

        // Act
        sut.Update(newName, newLocation, newOsType);

        // Assert
        sut.Name.Should().Be(newName);
        sut.Location.Should().Be(newLocation);
        sut.OsType.Value.Should().Be(AppServicePlanOsType.AppServicePlanOsTypeEnum.Linux);
    }

    [Fact]
    public void Given_NewEnvironment_When_SetEnvironmentSettings_Then_AddsEntry()
    {
        // Arrange
        var sut = CreateValidAppServicePlan();
        var sku = new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.S1);

        // Act
        sut.SetEnvironmentSettings("prod", sku, 3);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        var entry = sut.EnvironmentSettings.Single();
        entry.Sku.Should().Be(sku);
        entry.Capacity.Should().Be(3);
    }

    [Fact]
    public void Given_ExistingEnvironment_When_SetEnvironmentSettings_Then_UpdatesEntryInPlace()
    {
        // Arrange
        var sut = CreateValidAppServicePlan();
        sut.SetEnvironmentSettings("prod", new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.B1), 1);
        var newSku = new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.P2v3);

        // Act
        sut.SetEnvironmentSettings("prod", newSku, 5);

        // Assert
        sut.EnvironmentSettings.Should().ContainSingle();
        sut.EnvironmentSettings.Single().Sku.Should().Be(newSku);
        sut.EnvironmentSettings.Single().Capacity.Should().Be(5);
    }

    [Fact]
    public void Given_IsExistingResource_When_SetEnvironmentSettings_Then_DoesNothing()
    {
        // Arrange
        var sut = CreateValidAppServicePlan(isExisting: true);

        // Act
        sut.SetEnvironmentSettings("prod", new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.S1), 2);

        // Assert
        sut.EnvironmentSettings.Should().BeEmpty();
    }

    [Fact]
    public void Given_MultipleEnvironments_When_SetAllEnvironmentSettings_Then_ReplacesAll()
    {
        // Arrange
        var sut = CreateValidAppServicePlan();
        sut.SetEnvironmentSettings("dev", new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.B1), 1);
        var settings = new[]
        {
            ("staging", (AppServicePlanSku?)new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.S1), (int?)2),
            ("prod", (AppServicePlanSku?)new AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum.P1v3), (int?)3),
        };

        // Act
        sut.SetAllEnvironmentSettings(settings);

        // Assert
        sut.EnvironmentSettings.Should().HaveCount(2);
        sut.EnvironmentSettings.Should().NotContain(es => es.EnvironmentName == "dev");
    }
}
