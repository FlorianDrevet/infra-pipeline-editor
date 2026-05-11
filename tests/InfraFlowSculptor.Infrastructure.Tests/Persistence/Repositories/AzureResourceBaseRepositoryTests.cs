using InfraFlowSculptor.Domain.Common.BaseModels;
using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Repositories;

public sealed class AzureResourceBaseRepositoryTests
{
    private const string StaticAppSettingName = "ApiKey";
    private const string DevelopmentEnvironmentName = "dev";
    private const string ProductionEnvironmentName = "prod";
    private const string DevelopmentValue = "dev-value";
    private const string ProductionValue = "prod-value";
    private const string WebAppName = "wa-shared-001";
    private const string RuntimeVersion = "8.0";

    [Fact]
    public async Task Given_StaticAppSettingWithEnvironmentValues_When_GetByIdWithRoleAssignmentsAndAppSettingsAsync_Then_LoadsEnvironmentValues_Async()
    {
        // Arrange
        var databaseRoot = new InMemoryDatabaseRoot();
        var databaseName = $"test_{Guid.NewGuid()}";
        var resource = CreateWebApp();

        resource.AddStaticAppSetting(
            StaticAppSettingName,
            new Dictionary<string, string>
            {
                [DevelopmentEnvironmentName] = DevelopmentValue,
                [ProductionEnvironmentName] = ProductionValue,
            });

        await using (var seedContext = CreateContext(databaseName, databaseRoot))
        {
            await seedContext.AzureResources.AddAsync(resource);
            await seedContext.SaveChangesAsync();
        }

        AzureResource? result;

        // Act
        await using (var queryContext = CreateContext(databaseName, databaseRoot))
        {
            var sut = new AzureResourceBaseRepository(queryContext);
            result = await sut.GetByIdWithRoleAssignmentsAndAppSettingsAsync(resource.Id, CancellationToken.None);
        }

        // Assert
        result.Should().NotBeNull();
        var reloadedResource = result!;
        reloadedResource.AppSettings.Should().ContainSingle();

        var appSetting = reloadedResource.AppSettings.Single();
        appSetting.Name.Should().Be(StaticAppSettingName);
        appSetting.EnvironmentValues.Should().HaveCount(2);
        appSetting.EnvironmentValues.Should().ContainSingle(
            value => value.EnvironmentName == DevelopmentEnvironmentName && value.Value == DevelopmentValue);
        appSetting.EnvironmentValues.Should().ContainSingle(
            value => value.EnvironmentName == ProductionEnvironmentName && value.Value == ProductionValue);
    }

    private static ProjectDbContext CreateContext(string databaseName, InMemoryDatabaseRoot databaseRoot)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseInMemoryDatabase(databaseName, databaseRoot)
            .ConfigureWarnings(builder => builder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ProjectDbContext(options);
    }

    private static WebApp CreateWebApp()
    {
        return WebApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name(WebAppName),
            new Location(Location.LocationEnum.WestEurope),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            RuntimeVersion,
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null,
            dockerImageName: null);
    }
}