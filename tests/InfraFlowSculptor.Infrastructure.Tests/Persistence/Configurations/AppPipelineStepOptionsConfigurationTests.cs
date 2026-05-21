using FluentAssertions;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.OwnedEntities;
using InfraFlowSculptor.Domain.Common.OwnedEntities.Stacks;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;
using InfraFlowSculptor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Infrastructure.Tests.Persistence.Configurations;

public sealed class AppPipelineStepOptionsConfigurationTests
{
    private const int StackMaxLength = 30;
    private const int ToolMaxLength = 50;
    private const int CommandMaxLength = 500;

    [Fact]
    public void Given_AppPipelineStepOptionsOwnedModels_When_InspectingModel_Then_AllComputeOwnersPersistStackAndProfileFields()
    {
        // Arrange
        using var context = CreateRelationalContext();

        // Act
        var optionEntityTypes = context.Model.GetEntityTypes()
            .Where(static entityType => entityType.ClrType == typeof(AppPipelineStepOptions))
            .ToList();

        // Assert
        optionEntityTypes.Should().HaveCount(3);

        foreach (var entityType in optionEntityTypes)
        {
            AssertStringProperty(entityType, nameof(AppPipelineStepOptions.Stack), StackMaxLength);
            AssertStringProperty(entityType, "ProfileStack", StackMaxLength);
            AssertStringProperty(entityType, "DotNetProfileTestFramework", ToolMaxLength);
            AssertStringProperty(entityType, "DotNetProfileCustomTestProjectGlob", CommandMaxLength);
            AssertBooleanProperty(entityType, "DotNetProfileCollectCoverage");
        }
    }

    [Fact]
    public async Task Given_WebAppWithDotNetPipelineProfile_When_Reloaded_Then_ProfileIsRehydrated_Async()
    {
        // Arrange
        const string customTestProjectGlob = "tests/**/*.csproj";
        var databaseName = $"pipeline_profile_{Guid.NewGuid()}";
        var webApp = CreateWebAppWithDotNetProfile(customTestProjectGlob);

        await using (var arrangeContext = CreateInMemoryContext(databaseName))
        {
            arrangeContext.WebApps.Add(webApp);
            await arrangeContext.SaveChangesAsync();
        }

        // Act
        await using var assertContext = CreateInMemoryContext(databaseName);
        var stored = await assertContext.WebApps
            .AsNoTracking()
            .SingleAsync(entity => entity.Id == webApp.Id);

        // Assert
        stored.PipelineStepOptions.Stack.Should().Be(ApplicationStack.DotNet);
        var profile = stored.PipelineStepOptions.Profile.Should().BeOfType<DotNetPipelineProfile>().Subject;
        profile.TestFramework.Should().Be(DotNetTestFramework.NUnit);
        profile.CollectCoverage.Should().BeTrue();
        profile.CustomTestProjectGlob.Should().Be(customTestProjectGlob);
    }

    private static void AssertStringProperty(IReadOnlyEntityType entityType, string propertyName, int maxLength)
    {
        var property = entityType.FindProperty(propertyName);
        property.Should().NotBeNull($"{entityType.DisplayName()} should map {propertyName}");
        property!.GetMaxLength().Should().Be(maxLength);
        (property.GetTypeMapping().Converter?.ProviderClrType ?? property.ClrType).Should().Be(typeof(string));
    }

    private static void AssertBooleanProperty(IReadOnlyEntityType entityType, string propertyName)
    {
        var property = entityType.FindProperty(propertyName);
        property.Should().NotBeNull($"{entityType.DisplayName()} should map {propertyName}");
        property!.ClrType.Should().Be(typeof(bool?));
    }

    private static WebApp CreateWebAppWithDotNetProfile(string customTestProjectGlob)
    {
        var options = new AppPipelineStepOptions();
        options.Update(new AppPipelineStepOptionsData
        {
            Stack = ApplicationStack.DotNet,
            Profile = DotNetPipelineProfile.Create(
                DotNetTestFramework.NUnit,
                collectCoverage: true,
                customTestProjectGlob),
        });

        var webApp = WebApp.Create(
            ResourceGroupId.CreateUnique(),
            new Name("wa-pipeline-profile"),
            new Location(Location.LocationEnum.WestEurope),
            AzureResourceId.CreateUnique(),
            new WebAppRuntimeStack(WebAppRuntimeStack.WebAppRuntimeStackEnum.DotNet),
            "8.0",
            alwaysOn: true,
            httpsOnly: true,
            new DeploymentMode(DeploymentMode.DeploymentModeType.Code),
            containerRegistryId: null,
            acrAuthMode: null);

        webApp.SetPipelineStepOptions(options);
        return webApp;
    }

    private static ProjectDbContext CreateRelationalContext()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql("Host=localhost;Database=pipeline_profile_mapping_tests;Username=test;Password=test")
            .Options;

        return new ProjectDbContext(options);
    }

    private static ProjectDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(builder => builder.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ProjectDbContext(options);
    }
}