using FluentAssertions;
using InfraFlowSculptor.Application.Imports.Common.Creation;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Tests.Imports.Common.Creation;

public sealed class ResourceCommandFactoryOrderingTests
{
    [Fact]
    public void Given_IndependentResources_When_OrderByDependency_Then_PreservesRelativeOrder()
    {
        // Arrange
        var resources = new[]
        {
            (AzureResourceTypes.KeyVault, "kv"),
            (AzureResourceTypes.StorageAccount, "sa"),
            (AzureResourceTypes.CosmosDb, "cosmos"),
        };

        // Act
        var ordered = ResourceCommandFactory.OrderByDependency(resources);

        // Assert
        ordered.Select(r => r.ResourceType)
            .Should().ContainInOrder(AzureResourceTypes.KeyVault, AzureResourceTypes.StorageAccount, AzureResourceTypes.CosmosDb);
    }

    [Fact]
    public void Given_DependentBeforeDependency_When_OrderByDependency_Then_ReordersSoDependencyComesFirst()
    {
        // Arrange — WebApp listed before AppServicePlan, ContainerApp before its environment
        var resources = new[]
        {
            (AzureResourceTypes.WebApp, "webapp"),
            (AzureResourceTypes.ContainerApp, "ca"),
            (AzureResourceTypes.AppServicePlan, "asp"),
            (AzureResourceTypes.ContainerAppEnvironment, "cae"),
            (AzureResourceTypes.KeyVault, "kv"),
        };

        // Act
        var ordered = ResourceCommandFactory.OrderByDependency(resources).ToList();

        // Assert
        IndexOf(ordered, AzureResourceTypes.AppServicePlan)
            .Should().BeLessThan(IndexOf(ordered, AzureResourceTypes.WebApp));
        IndexOf(ordered, AzureResourceTypes.ContainerAppEnvironment)
            .Should().BeLessThan(IndexOf(ordered, AzureResourceTypes.ContainerApp));
    }

    [Fact]
    public void Given_MultiLevelDependencyChain_When_OrderByDependency_Then_RespectsTransitiveOrder()
    {
        // ApplicationInsights -> LogAnalyticsWorkspace; SqlDatabase -> SqlServer
        var resources = new[]
        {
            (AzureResourceTypes.ApplicationInsights, "appi"),
            (AzureResourceTypes.SqlDatabase, "sqldb"),
            (AzureResourceTypes.LogAnalyticsWorkspace, "law"),
            (AzureResourceTypes.SqlServer, "sql"),
        };

        var ordered = ResourceCommandFactory.OrderByDependency(resources).ToList();

        IndexOf(ordered, AzureResourceTypes.LogAnalyticsWorkspace)
            .Should().BeLessThan(IndexOf(ordered, AzureResourceTypes.ApplicationInsights));
        IndexOf(ordered, AzureResourceTypes.SqlServer)
            .Should().BeLessThan(IndexOf(ordered, AzureResourceTypes.SqlDatabase));
    }

    [Fact]
    public void Given_DependentWithoutDependencyInList_When_OrderByDependency_Then_DependentIsKept()
    {
        // Arrange
        var resources = new[]
        {
            (AzureResourceTypes.WebApp, "webapp"),
            (AzureResourceTypes.KeyVault, "kv"),
        };

        // Act
        var ordered = ResourceCommandFactory.OrderByDependency(resources).ToList();

        // Assert
        ordered.Should().HaveCount(2);
        ordered.Should().Contain(r => r.ResourceType == AzureResourceTypes.WebApp);
    }

    [Fact]
    public void Given_DuplicateResources_When_OrderByDependency_Then_KeepsAllOccurrences()
    {
        // Arrange — two WebApps both depending on the same plan
        var resources = new[]
        {
            (AzureResourceTypes.WebApp, "webapp-1"),
            (AzureResourceTypes.WebApp, "webapp-2"),
            (AzureResourceTypes.AppServicePlan, "asp"),
        };

        // Act
        var ordered = ResourceCommandFactory.OrderByDependency(resources).ToList();

        // Assert
        ordered.Should().HaveCount(3);
        var aspIndex = IndexOf(ordered, AzureResourceTypes.AppServicePlan);
        ordered.Where(r => r.ResourceType == AzureResourceTypes.WebApp)
            .Should().HaveCount(2);
        aspIndex.Should().BeLessThan(
            ordered.FindIndex(r => r.Name == "webapp-1"));
        aspIndex.Should().BeLessThan(
            ordered.FindIndex(r => r.Name == "webapp-2"));
    }

    private static int IndexOf(IReadOnlyList<(string ResourceType, string Name)> ordered, string type)
    {
        for (var i = 0; i < ordered.Count; i++)
        {
            if (string.Equals(ordered[i].ResourceType, type, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }
}
