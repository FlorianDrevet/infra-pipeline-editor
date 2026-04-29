using ErrorOr;
using FluentAssertions;
using InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.Application.Imports.Common.Properties;
using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Application.RedisCaches.Commands.CreateRedisCache;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.GenerationCore;
using MediatR;
using NSubstitute;

namespace InfraFlowSculptor.Application.Tests.Imports.Common.Creation;

/// <summary>
/// Verifies the resource-type → command mapping. Each independent type is dispatched and the mediator
/// receives an instance of the expected command type. Dependent types are verified to refuse dispatch
/// when their dependency is missing and to dispatch the right command when the dependency is provided.
/// </summary>
public sealed class ResourceCommandFactoryDispatchMappingTests
{
    private static readonly ResourceGroupId TestRgId = new(Guid.NewGuid());
    private static readonly Name TestName = new("test-resource");
    private static readonly Location TestLocation = new(Location.LocationEnum.WestEurope);

    /// <summary>
    /// Configures the mediator substitute to return an error for a given command type,
    /// so tests can properly await the dispatch and assert the async flow completes.
    /// </summary>
    private static void ConfigureErrorReturn<TCommand, TResult>(ISender mediator)
        where TCommand : IRequest<ErrorOr<TResult>>
    {
        mediator.Send(Arg.Any<TCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<TResult>>(
                Error.Unexpected("test.mock", $"Mock dispatch for {typeof(TCommand).Name}")));
    }

    private static ISender CreateConfiguredMediator()
    {
        var mediator = Substitute.For<ISender>();

        ConfigureErrorReturn<CreateKeyVaultCommand, KeyVaultResult>(mediator);
        ConfigureErrorReturn<CreateStorageAccountCommand, StorageAccountResult>(mediator);
        ConfigureErrorReturn<CreateAppServicePlanCommand, AppServicePlanResult>(mediator);
        ConfigureErrorReturn<CreateRedisCacheCommand, RedisCacheResult>(mediator);
        ConfigureErrorReturn<CreateUserAssignedIdentityCommand, UserAssignedIdentityResult>(mediator);
        ConfigureErrorReturn<CreateAppConfigurationCommand, AppConfigurationResult>(mediator);
        ConfigureErrorReturn<CreateLogAnalyticsWorkspaceCommand, LogAnalyticsWorkspaceResult>(mediator);
        ConfigureErrorReturn<CreateCosmosDbCommand, CosmosDbResult>(mediator);
        ConfigureErrorReturn<CreateSqlServerCommand, SqlServerResult>(mediator);
        ConfigureErrorReturn<CreateServiceBusNamespaceCommand, ServiceBusNamespaceResult>(mediator);
        ConfigureErrorReturn<CreateContainerRegistryCommand, ContainerRegistryResult>(mediator);
        ConfigureErrorReturn<CreateEventHubNamespaceCommand, EventHubNamespaceResult>(mediator);
        ConfigureErrorReturn<CreateContainerAppEnvironmentCommand, ContainerAppEnvironmentResult>(mediator);
        ConfigureErrorReturn<CreateWebAppCommand, WebAppResult>(mediator);
        ConfigureErrorReturn<CreateFunctionAppCommand, FunctionAppResult>(mediator);
        ConfigureErrorReturn<CreateContainerAppCommand, ContainerAppResult>(mediator);
        ConfigureErrorReturn<CreateApplicationInsightsCommand, ApplicationInsightsResult>(mediator);
        ConfigureErrorReturn<CreateSqlDatabaseCommand, SqlDatabaseResult>(mediator);

        return mediator;
    }

    public static TheoryData<string, Type> IndependentResourceTypes() => new()
    {
        { AzureResourceTypes.KeyVault, typeof(CreateKeyVaultCommand) },
        { AzureResourceTypes.StorageAccount, typeof(CreateStorageAccountCommand) },
        { AzureResourceTypes.AppServicePlan, typeof(CreateAppServicePlanCommand) },
        { AzureResourceTypes.RedisCache, typeof(CreateRedisCacheCommand) },
        { AzureResourceTypes.UserAssignedIdentity, typeof(CreateUserAssignedIdentityCommand) },
        { AzureResourceTypes.AppConfiguration, typeof(CreateAppConfigurationCommand) },
        { AzureResourceTypes.LogAnalyticsWorkspace, typeof(CreateLogAnalyticsWorkspaceCommand) },
        { AzureResourceTypes.CosmosDb, typeof(CreateCosmosDbCommand) },
        { AzureResourceTypes.SqlServer, typeof(CreateSqlServerCommand) },
        { AzureResourceTypes.ServiceBusNamespace, typeof(CreateServiceBusNamespaceCommand) },
        { AzureResourceTypes.ContainerRegistry, typeof(CreateContainerRegistryCommand) },
        { AzureResourceTypes.EventHubNamespace, typeof(CreateEventHubNamespaceCommand) },
        { AzureResourceTypes.ContainerAppEnvironment, typeof(CreateContainerAppEnvironmentCommand) },
    };

    [Theory]
    [MemberData(nameof(IndependentResourceTypes))]
    public async Task Given_IndependentResourceType_When_CreateResourceAsync_Then_DispatchesExpectedCommand(
        string resourceType, Type expectedCommandType)
    {
        // Arrange
        var mediator = CreateConfiguredMediator();
        var context = new ResourceCreationContext(new Dictionary<string, Guid>());

        // Act
        var task = ResourceCommandFactory.CreateResourceAsync(
            mediator, resourceType, TestRgId, TestName, TestLocation, context, CancellationToken.None);

        // Assert
        task.Should().NotBeNull();
        var result = await task!;
        result.IsError.Should().BeTrue("mediator is configured to return errors for mapping verification");
        mediator.ReceivedCalls()
            .Should().Contain(call => call.GetArguments()[0]!.GetType() == expectedCommandType);
    }

    [Theory]
    [InlineData(AzureResourceTypes.WebApp)]
    [InlineData(AzureResourceTypes.FunctionApp)]
    [InlineData(AzureResourceTypes.ContainerApp)]
    [InlineData(AzureResourceTypes.ApplicationInsights)]
    [InlineData(AzureResourceTypes.SqlDatabase)]
    public void Given_DependentTypeWithoutDependency_When_CreateResourceAsync_Then_ReturnsNull(string resourceType)
    {
        // Arrange
        var mediator = Substitute.For<ISender>();
        var context = new ResourceCreationContext(new Dictionary<string, Guid>());

        // Act
        var task = ResourceCommandFactory.CreateResourceAsync(
            mediator, resourceType, TestRgId, TestName, TestLocation, context, CancellationToken.None);

        // Assert
        task.Should().BeNull();
        mediator.ReceivedCalls().Should().BeEmpty();
    }

    [Theory]
    [InlineData(AzureResourceTypes.WebApp, AzureResourceTypes.AppServicePlan, typeof(CreateWebAppCommand))]
    [InlineData(AzureResourceTypes.FunctionApp, AzureResourceTypes.AppServicePlan, typeof(CreateFunctionAppCommand))]
    [InlineData(AzureResourceTypes.ContainerApp, AzureResourceTypes.ContainerAppEnvironment, typeof(CreateContainerAppCommand))]
    [InlineData(AzureResourceTypes.ApplicationInsights, AzureResourceTypes.LogAnalyticsWorkspace, typeof(CreateApplicationInsightsCommand))]
    [InlineData(AzureResourceTypes.SqlDatabase, AzureResourceTypes.SqlServer, typeof(CreateSqlDatabaseCommand))]
    public async Task Given_DependentTypeWithDependency_When_CreateResourceAsync_Then_DispatchesExpectedCommand(
        string resourceType, string dependencyType, Type expectedCommandType)
    {
        // Arrange
        var mediator = CreateConfiguredMediator();
        var context = new ResourceCreationContext(new Dictionary<string, Guid>
        {
            [dependencyType] = Guid.NewGuid(),
        });

        // Act
        var task = ResourceCommandFactory.CreateResourceAsync(
            mediator, resourceType, TestRgId, TestName, TestLocation, context, CancellationToken.None);

        // Assert
        task.Should().NotBeNull();
        var result = await task!;
        result.IsError.Should().BeTrue("mediator is configured to return errors for mapping verification");
        mediator.ReceivedCalls()
            .Should().Contain(call => call.GetArguments()[0]!.GetType() == expectedCommandType);
    }

    [Fact]
    public async Task Given_StorageAccountExtractedProperties_When_CreateResourceAsync_Then_DispatchesCommandWithExtractedKind()
    {
        // Arrange
        var mediator = CreateConfiguredMediator();
        var props = new StorageAccountExtractedProperties("Standard_LRS", "BlobStorage", []);
        var context = new ResourceCreationContext(new Dictionary<string, Guid>(), TypedProperties: props);

        // Act
        var task = ResourceCommandFactory.CreateResourceAsync(
            mediator, AzureResourceTypes.StorageAccount, TestRgId, TestName, TestLocation, context, CancellationToken.None);

        // Assert
        task.Should().NotBeNull();
        var result = await task!;
        result.IsError.Should().BeTrue("mediator is configured to return errors for mapping verification");

        var call = mediator.ReceivedCalls()
            .Single(c => c.GetArguments()[0] is CreateStorageAccountCommand);
        var command = (CreateStorageAccountCommand)call.GetArguments()[0]!;
        command.Kind.Should().Be("BlobStorage");
    }

    [Fact]
    public void Given_UnknownResourceType_When_CreateResourceAsync_Then_ReturnsNull()
    {
        // Arrange
        var mediator = Substitute.For<ISender>();
        var context = new ResourceCreationContext(new Dictionary<string, Guid>());

        // Act
        var task = ResourceCommandFactory.CreateResourceAsync(
            mediator, "UnknownType", TestRgId, TestName, TestLocation, context, CancellationToken.None);

        // Assert
        task.Should().BeNull();
        mediator.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void Given_DependentTypeWithoutDependency_When_GetMissingDependency_Then_ReturnsDependencyType()
    {
        var missing = ResourceCommandFactory.GetMissingDependency(
            AzureResourceTypes.WebApp, new Dictionary<string, Guid>());

        missing.Should().Be(AzureResourceTypes.AppServicePlan);
    }

    [Fact]
    public void Given_IndependentType_When_GetMissingDependency_Then_ReturnsNull()
    {
        var missing = ResourceCommandFactory.GetMissingDependency(
            AzureResourceTypes.KeyVault, new Dictionary<string, Guid>());

        missing.Should().BeNull();
    }

    [Fact]
    public void Given_AllAzureResourceTypes_When_ComparedToTestCoverage_Then_AllTypesAreCovered()
    {
        // ResourceGroup is a container type, not dispatched by the factory.
        var excludedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { AzureResourceTypes.ResourceGroup };

        var allTypes = typeof(AzureResourceTypes)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Where(t => !excludedTypes.Contains(t))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var testedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in IndependentResourceTypes())
            testedTypes.Add((string)row[0]);

        testedTypes.Add(AzureResourceTypes.WebApp);
        testedTypes.Add(AzureResourceTypes.FunctionApp);
        testedTypes.Add(AzureResourceTypes.ContainerApp);
        testedTypes.Add(AzureResourceTypes.ApplicationInsights);
        testedTypes.Add(AzureResourceTypes.SqlDatabase);

        testedTypes.Should().BeEquivalentTo(allTypes, "every AzureResourceTypes constant must have dispatch test coverage");
    }

    [Theory]
    [InlineData(AzureResourceTypes.WebApp, AzureResourceTypes.AppServicePlan)]
    [InlineData(AzureResourceTypes.FunctionApp, AzureResourceTypes.AppServicePlan)]
    [InlineData(AzureResourceTypes.ContainerApp, AzureResourceTypes.ContainerAppEnvironment)]
    [InlineData(AzureResourceTypes.ApplicationInsights, AzureResourceTypes.LogAnalyticsWorkspace)]
    [InlineData(AzureResourceTypes.SqlDatabase, AzureResourceTypes.SqlServer)]
    public void Given_DependentType_When_GetRequiredDependencyType_Then_ReturnsDependencyType(
        string resourceType, string expectedDependencyType)
    {
        ResourceCommandFactory.GetRequiredDependencyType(resourceType).Should().Be(expectedDependencyType);
    }

    [Fact]
    public void Given_IndependentType_When_GetRequiredDependencyType_Then_ReturnsNull()
    {
        ResourceCommandFactory.GetRequiredDependencyType(AzureResourceTypes.KeyVault).Should().BeNull();
    }
}
