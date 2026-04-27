using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.BicepGeneration.Pipeline;
using InfraFlowSculptor.BicepGeneration.Pipeline.Stages;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Pipeline.Stages;

public sealed class ParentReferenceResolutionStageTests
{
    private readonly ParentReferenceResolutionStage _sut = new();

    [Fact]
    public void Given_Stage_When_CheckOrder_Then_Returns800()
    {
        _sut.Order.Should().Be(800);
    }

    [Fact]
    public void Given_ResourceWithAppServicePlanId_When_Execute_Then_ParentModuleIdRefResolved()
    {
        // Arrange
        var aspId = Guid.NewGuid();
        var webAppResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-webapp",
            Type = "Microsoft.Web/sites",
            Properties = new Dictionary<string, string> { ["appServicePlanId"] = aspId.ToString() },
        };
        var context = CreateContext([webAppResource],
            new Dictionary<Guid, (string, string)> { [aspId] = ("my-asp", AzureResourceTypes.AppServicePlan) });

        // Act
        _sut.Execute(context);

        // Assert
        var module = context.WorkItems[0].Module;
        module.ParentModuleIdReferences.Should().ContainKey("appServicePlanId");
        module.ParentModuleIdReferences["appServicePlanId"].Name.Should().Be("my-asp");
    }

    [Fact]
    public void Given_ResourceWithContainerAppEnvironmentId_When_Execute_Then_ParentModuleIdRefResolved()
    {
        // Arrange
        var caeId = Guid.NewGuid();
        var containerResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-container",
            Type = "Microsoft.App/containerApps",
            Properties = new Dictionary<string, string> { ["containerAppEnvironmentId"] = caeId.ToString() },
        };
        var context = CreateContext([containerResource],
            new Dictionary<Guid, (string, string)> { [caeId] = ("my-cae", AzureResourceTypes.ContainerAppEnvironment) });

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ParentModuleIdReferences
            .Should().ContainKey("containerAppEnvironmentId");
    }

    [Fact]
    public void Given_ResourceWithSqlServerId_When_Execute_Then_ParentModuleNameRefResolved()
    {
        // Arrange
        var sqlId = Guid.NewGuid();
        var sqlDbResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-db",
            Type = "Microsoft.Sql/servers/databases",
            Properties = new Dictionary<string, string> { ["sqlServerId"] = sqlId.ToString() },
        };
        var context = CreateContext([sqlDbResource],
            new Dictionary<Guid, (string, string)> { [sqlId] = ("my-sql", AzureResourceTypes.SqlServer) });

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ParentModuleNameReferences
            .Should().ContainKey("sqlServerName");
        context.WorkItems[0].Module.ParentModuleNameReferences["sqlServerName"].Name
            .Should().Be("my-sql");
    }

    [Fact]
    public void Given_AppInsightsWithoutLawId_When_Execute_Then_FallsBackToInConfigLaw()
    {
        // Arrange
        var appInsightsResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-ai",
            Type = "Microsoft.Insights/components",
            Properties = new Dictionary<string, string>(),
        };
        var lawResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-law",
            Type = "Microsoft.OperationalInsights/workspaces",
        };
        var context = CreateContextWithResources(
            [appInsightsResource, lawResource],
            workItemResource: appInsightsResource);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ParentModuleIdReferences
            .Should().ContainKey("logAnalyticsWorkspaceId");
        context.WorkItems[0].Module.ParentModuleIdReferences["logAnalyticsWorkspaceId"].Name
            .Should().Be("my-law");
    }

    [Fact]
    public void Given_AppInsightsWithoutLawIdOrInConfigLaw_When_Execute_Then_FallsBackToExistingRef()
    {
        // Arrange
        var appInsightsResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-ai",
            Type = "Microsoft.Insights/components",
            Properties = new Dictionary<string, string>(),
        };
        var existingRef = new ExistingResourceReference
        {
            ResourceName = "shared-law",
            ResourceType = "Microsoft.OperationalInsights/workspaces",
            ResourceTypeName = AzureResourceTypes.LogAnalyticsWorkspace,
        };

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest
            {
                Resources = [appInsightsResource],
                ExistingResourceReferences = [existingRef],
            },
            ResourceIdToInfo = [],
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = appInsightsResource,
            Module = new GeneratedTypeModule(),
        });

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ExistingResourceIdReferences
            .Should().ContainKey("logAnalyticsWorkspaceId");
        context.WorkItems[0].Module.ExistingResourceIdReferences["logAnalyticsWorkspaceId"]
            .Should().Be("shared-law");
    }

    [Fact]
    public void Given_ResourceWithNoParentProperties_When_Execute_Then_AllRefsEmpty()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-kv",
            Type = "Microsoft.KeyVault/vaults",
            Properties = new Dictionary<string, string>(),
        };
        var context = CreateContext([resource], []);

        // Act
        _sut.Execute(context);

        // Assert
        var module = context.WorkItems[0].Module;
        module.ParentModuleIdReferences.Should().BeEmpty();
        module.ParentModuleNameReferences.Should().BeEmpty();
        module.ExistingResourceIdReferences.Should().BeEmpty();
    }

    [Fact]
    public void Given_ResourceWithIdentityKindOnWorkItem_When_Execute_Then_PropagatedToModule()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-kv",
            Type = "Microsoft.KeyVault/vaults",
            Properties = new Dictionary<string, string>(),
        };
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = [resource] },
            ResourceIdToInfo = [],
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = resource,
            Module = new GeneratedTypeModule(),
            IdentityKind = "SystemAssigned",
            UsesParameterizedIdentity = true,
        });

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.IdentityKind.Should().Be("SystemAssigned");
        context.WorkItems[0].Module.UsesParameterizedIdentity.Should().BeTrue();
    }

    [Fact]
    public void Given_UnresolvableParentGuid_When_Execute_Then_SilentlyDropped()
    {
        // Arrange
        var resource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-webapp",
            Type = "Microsoft.Web/sites",
            Properties = new Dictionary<string, string>
            {
                ["appServicePlanId"] = Guid.NewGuid().ToString(), // not in resourceIdToInfo
            },
        };
        var context = CreateContext([resource], []);

        // Act
        _sut.Execute(context);

        // Assert
        context.WorkItems[0].Module.ParentModuleIdReferences.Should().BeEmpty();
    }

    // ── Helpers ──

    private static BicepGenerationContext CreateContext(
        ResourceDefinition[] resources,
        Dictionary<Guid, (string Name, string ResourceTypeName)> resourceIdToInfo)
    {
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = resources },
            ResourceIdToInfo = resourceIdToInfo,
        };
        foreach (var resource in resources)
        {
            context.WorkItems.Add(new ModuleWorkItem
            {
                Resource = resource,
                Module = new GeneratedTypeModule(),
            });
        }
        return context;
    }

    private static BicepGenerationContext CreateContextWithResources(
        ResourceDefinition[] allResources,
        ResourceDefinition workItemResource)
    {
        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest { Resources = allResources },
            ResourceIdToInfo = [],
        };
        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = workItemResource,
            Module = new GeneratedTypeModule(),
        });
        return context;
    }
}
