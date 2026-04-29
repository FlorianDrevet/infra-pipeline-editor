using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;
using InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;
using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.Application.Imports.Common.Properties;

namespace InfraFlowSculptor.Mcp.Tests.Tools;

public sealed class ResourceCommandFactoryTests
{
    private static readonly ResourceGroupId TestRgId = new(Guid.NewGuid());
    private static readonly Name TestName = new("test-resource");
    private static readonly Location TestLocation = new(Location.LocationEnum.WestEurope);
    private static readonly Dictionary<string, Guid> EmptyCreated = new();

    // ── Simple resource types ──────────────────────────────────────────

    [Theory]
    [InlineData(AzureResourceTypes.KeyVault, typeof(CreateKeyVaultCommand))]
    [InlineData(AzureResourceTypes.StorageAccount, typeof(CreateStorageAccountCommand))]
    [InlineData(AzureResourceTypes.AppServicePlan, typeof(CreateAppServicePlanCommand))]
    [InlineData(AzureResourceTypes.RedisCache, typeof(CreateRedisCacheCommand))]
    [InlineData(AzureResourceTypes.UserAssignedIdentity, typeof(CreateUserAssignedIdentityCommand))]
    [InlineData(AzureResourceTypes.AppConfiguration, typeof(CreateAppConfigurationCommand))]
    [InlineData(AzureResourceTypes.LogAnalyticsWorkspace, typeof(CreateLogAnalyticsWorkspaceCommand))]
    [InlineData(AzureResourceTypes.CosmosDb, typeof(CreateCosmosDbCommand))]
    [InlineData(AzureResourceTypes.SqlServer, typeof(CreateSqlServerCommand))]
    [InlineData(AzureResourceTypes.ServiceBusNamespace, typeof(CreateServiceBusNamespaceCommand))]
    [InlineData(AzureResourceTypes.ContainerRegistry, typeof(CreateContainerRegistryCommand))]
    [InlineData(AzureResourceTypes.EventHubNamespace, typeof(CreateEventHubNamespaceCommand))]
    [InlineData(AzureResourceTypes.ContainerAppEnvironment, typeof(CreateContainerAppEnvironmentCommand))]
    public void BuildCommand_IndependentResourceType_ReturnsCorrectCommandType(string resourceType, Type expectedType)
    {
        // Act
        var command = ResourceCommandFactory.BuildCommand(
            resourceType, TestRgId, TestName, TestLocation, new ResourceCreationContext(EmptyCreated));

        // Assert
        command.Should().NotBeNull();
        command.Should().BeOfType(expectedType);
    }

    // ── Dependent resource types without dependency ────────────────────

    [Theory]
    [InlineData(AzureResourceTypes.WebApp)]
    [InlineData(AzureResourceTypes.FunctionApp)]
    [InlineData(AzureResourceTypes.ContainerApp)]
    [InlineData(AzureResourceTypes.ApplicationInsights)]
    [InlineData(AzureResourceTypes.SqlDatabase)]
    public void BuildCommand_DependentResourceType_WithoutDependency_ReturnsNull(string resourceType)
    {
        // Act
        var command = ResourceCommandFactory.BuildCommand(
            resourceType, TestRgId, TestName, TestLocation, new ResourceCreationContext(EmptyCreated));

        // Assert
        command.Should().BeNull();
    }

    // ── Dependent resource types with dependency ───────────────────────

    [Fact]
    public void BuildCommand_WebApp_WithAppServicePlan_ReturnsCommand()
    {
        // Arrange
        var created = new Dictionary<string, Guid>
        {
            [AzureResourceTypes.AppServicePlan] = Guid.NewGuid(),
        };

        // Act
        var command = ResourceCommandFactory.BuildCommand(
            AzureResourceTypes.WebApp, TestRgId, TestName, TestLocation, new ResourceCreationContext(created));

        // Assert
        command.Should().BeOfType<CreateWebAppCommand>();
        var webApp = (CreateWebAppCommand)command!;
        webApp.AppServicePlanId.Should().Be(created[AzureResourceTypes.AppServicePlan]);
    }

    [Fact]
    public void BuildCommand_ContainerApp_WithEnvironment_ReturnsCommand()
    {
        // Arrange
        var created = new Dictionary<string, Guid>
        {
            [AzureResourceTypes.ContainerAppEnvironment] = Guid.NewGuid(),
        };

        // Act
        var command = ResourceCommandFactory.BuildCommand(
            AzureResourceTypes.ContainerApp, TestRgId, TestName, TestLocation, new ResourceCreationContext(created));

        // Assert
        command.Should().BeOfType<CreateContainerAppCommand>();
    }

    [Fact]
    public void BuildCommand_ApplicationInsights_WithLogAnalytics_ReturnsCommand()
    {
        // Arrange
        var created = new Dictionary<string, Guid>
        {
            [AzureResourceTypes.LogAnalyticsWorkspace] = Guid.NewGuid(),
        };

        // Act
        var command = ResourceCommandFactory.BuildCommand(
            AzureResourceTypes.ApplicationInsights, TestRgId, TestName, TestLocation, new ResourceCreationContext(created));

        // Assert
        command.Should().BeOfType<CreateApplicationInsightsCommand>();
    }

    // ── Unknown resource type ──────────────────────────────────────────

    [Fact]
    public void BuildCommand_UnknownResourceType_ReturnsNull()
    {
        var command = ResourceCommandFactory.BuildCommand(
            "UnknownType", TestRgId, TestName, TestLocation, new ResourceCreationContext(EmptyCreated));

        command.Should().BeNull();
    }

    // ── Extracted properties ───────────────────────────────────────────

    [Fact]
    public void BuildCommand_StorageAccount_UsesExtractedProperties()
    {
        // Arrange
        var props = new StorageAccountExtractedProperties("Standard_LRS", "BlobStorage", []);

        // Act
        var command = ResourceCommandFactory.BuildCommand(
            AzureResourceTypes.StorageAccount, TestRgId, TestName, TestLocation,
            new ResourceCreationContext(EmptyCreated, TypedProperties: props));

        // Assert
        var sa = command.Should().BeOfType<CreateStorageAccountCommand>().Subject;
        sa.Kind.Should().Be("BlobStorage");
    }

    // ── OrderByDependency ──────────────────────────────────────────────

    [Fact]
    public void OrderByDependency_IndependentResources_PreserveOrder()
    {
        // Arrange
        var resources = new[]
        {
            ("KeyVault", "kv"),
            ("StorageAccount", "sa"),
        };

        // Act
        var ordered = ResourceCommandFactory.OrderByDependency(resources);

        // Assert
        ordered.Should().HaveCount(2);
        ordered[0].ResourceType.Should().Be("KeyVault");
        ordered[1].ResourceType.Should().Be("StorageAccount");
    }

    [Fact]
    public void OrderByDependency_DependentBeforeIndependent_ReordersCorrectly()
    {
        // Arrange — WebApp listed before AppServicePlan
        var resources = new[]
        {
            (AzureResourceTypes.WebApp, "webapp"),
            (AzureResourceTypes.AppServicePlan, "asp"),
            (AzureResourceTypes.KeyVault, "kv"),
        };

        // Act
        var ordered = ResourceCommandFactory.OrderByDependency(resources);

        // Assert
        var aspIndex = ordered.ToList().FindIndex(r => r.ResourceType == AzureResourceTypes.AppServicePlan);
        var webAppIndex = ordered.ToList().FindIndex(r => r.ResourceType == AzureResourceTypes.WebApp);
        aspIndex.Should().BeLessThan(webAppIndex, "AppServicePlan should be created before WebApp");
    }

    [Fact]
    public void OrderByDependency_DependentWithoutDependency_StaysIndependent()
    {
        // WebApp without AppServicePlan in the list → treated as independent
        var resources = new[]
        {
            (AzureResourceTypes.WebApp, "webapp"),
            (AzureResourceTypes.KeyVault, "kv"),
        };

        var ordered = ResourceCommandFactory.OrderByDependency(resources);
        ordered.Should().HaveCount(2);
    }

    // ── GetMissingDependency ───────────────────────────────────────────

    [Fact]
    public void GetMissingDependency_WebApp_WithoutPlan_ReturnsMissing()
    {
        var missing = ResourceCommandFactory.GetMissingDependency(
            AzureResourceTypes.WebApp, EmptyCreated);

        missing.Should().Be(AzureResourceTypes.AppServicePlan);
    }

    [Fact]
    public void GetMissingDependency_KeyVault_ReturnsNull()
    {
        var missing = ResourceCommandFactory.GetMissingDependency(
            AzureResourceTypes.KeyVault, EmptyCreated);

        missing.Should().BeNull();
    }
}
