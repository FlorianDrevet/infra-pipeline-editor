using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Ir;
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
    public void Given_ContainerAppWithInConfigContainerRegistry_When_Execute_Then_ComputesAcrLoginServerFromParentModuleOutput()
    {
        // Arrange
        var containerRegistryId = Guid.NewGuid();
        var containerAppResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "ifs-frontend",
            Type = AzureResourceTypes.ArmTypes.ContainerAppType,
            Properties = new Dictionary<string, string>
            {
                ["containerRegistryId"] = containerRegistryId.ToString(),
            },
        };

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest
            {
                Resources =
                [
                    containerAppResource,
                    new ResourceDefinition
                    {
                        ResourceId = containerRegistryId,
                        Name = "infraflowsculptor",
                        Type = AzureResourceTypes.ArmTypes.ContainerRegistryType,
                    },
                ],
            },
            ResourceIdToInfo = new Dictionary<Guid, (string Name, string ResourceTypeName)>
            {
                [containerRegistryId] = ("infraflowsculptor", AzureResourceTypes.ContainerRegistry),
            },
        };

        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = containerAppResource,
            Module = new GeneratedTypeModule
            {
                Parameters = new Dictionary<string, object>
                {
                    ["acrLoginServer"] = string.Empty,
                    ["acrManagedIdentityClientId"] = string.Empty,
                },
            },
            Spec = CreateMinimalSpec(),
        });

        // Act
        _sut.Execute(context);

        // Assert
        var module = context.WorkItems[0].Module;
        module.ParentModuleOutputReferences.Should().ContainKey("acrLoginServer");
        module.ParentModuleOutputReferences["acrLoginServer"].Name.Should().Be("infraflowsculptor");
        module.ParentModuleOutputReferences["acrLoginServer"].ResourceTypeName.Should().Be(AzureResourceTypes.ContainerRegistry);
        module.ParentModuleOutputReferences["acrLoginServer"].OutputName.Should().Be("loginServer");
        module.Parameters.Should().NotContainKey("acrLoginServer");
        module.Parameters.Should().ContainKey("acrManagedIdentityClientId");
    }

    [Fact]
    public void Given_ContainerAppWithCrossConfigContainerRegistry_When_Execute_Then_ComputesAcrLoginServerFromExistingResourceProperty()
    {
        // Arrange
        var containerAppResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "ifs-frontend",
            Type = AzureResourceTypes.ArmTypes.ContainerAppType,
            Properties = new Dictionary<string, string>
            {
                ["containerRegistryId"] = Guid.NewGuid().ToString(),
            },
        };

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest
            {
                Resources = [containerAppResource],
                ExistingResourceReferences =
                [
                    new ExistingResourceReference
                    {
                        ResourceName = "infraflowsculptor",
                        ResourceTypeName = AzureResourceTypes.ContainerRegistry,
                        ResourceType = AzureResourceTypes.ArmTypes.ContainerRegistryType,
                        ResourceGroupName = "ifs-core",
                        ResourceAbbreviation = "acr",
                    },
                ],
            },
            ResourceIdToInfo = [],
        };

        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = containerAppResource,
            Module = new GeneratedTypeModule
            {
                Parameters = new Dictionary<string, object>
                {
                    ["acrLoginServer"] = string.Empty,
                    ["acrManagedIdentityClientId"] = string.Empty,
                },
            },
            Spec = CreateMinimalSpec(),
        });

        // Act
        _sut.Execute(context);

        // Assert
        var module = context.WorkItems[0].Module;
        module.ExistingResourcePropertyReferences.Should().ContainKey("acrLoginServer");
        module.ExistingResourcePropertyReferences["acrLoginServer"].ResourceName.Should().Be("infraflowsculptor");
        module.ExistingResourcePropertyReferences["acrLoginServer"].PropertyPath.Should().Be("properties.loginServer");
        module.Parameters.Should().NotContainKey("acrLoginServer");
        module.Parameters.Should().ContainKey("acrManagedIdentityClientId");
    }

    [Fact]
    public void Given_ContainerAppWithMultipleExistingContainerRegistries_When_Execute_Then_UsesMatchingTargetResourceIdForAcrLoginServer()
    {
        // Arrange
        var firstContainerRegistryId = Guid.NewGuid();
        var secondContainerRegistryId = Guid.NewGuid();

        var containerAppResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "ifs-worker",
            Type = AzureResourceTypes.ArmTypes.ContainerAppType,
            Properties = new Dictionary<string, string>
            {
                ["containerRegistryId"] = secondContainerRegistryId.ToString(),
            },
        };

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest
            {
                Resources = [containerAppResource],
                ExistingResourceReferences =
                [
                    new ExistingResourceReference
                    {
                        ResourceName = "acr-primary",
                        ResourceTypeName = AzureResourceTypes.ContainerRegistry,
                        ResourceType = AzureResourceTypes.ArmTypes.ContainerRegistryType,
                        ResourceGroupName = "rg-primary",
                        ResourceAbbreviation = "acr",
                        TargetResourceId = firstContainerRegistryId,
                    },
                    new ExistingResourceReference
                    {
                        ResourceName = "acr-secondary",
                        ResourceTypeName = AzureResourceTypes.ContainerRegistry,
                        ResourceType = AzureResourceTypes.ArmTypes.ContainerRegistryType,
                        ResourceGroupName = "rg-secondary",
                        ResourceAbbreviation = "acr",
                        TargetResourceId = secondContainerRegistryId,
                    },
                ],
            },
            ResourceIdToInfo = [],
        };

        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = containerAppResource,
            Module = new GeneratedTypeModule
            {
                Parameters = new Dictionary<string, object>
                {
                    ["acrLoginServer"] = string.Empty,
                    ["acrManagedIdentityClientId"] = string.Empty,
                },
            },
            Spec = CreateMinimalSpec(),
        });

        // Act
        _sut.Execute(context);

        // Assert
        var module = context.WorkItems[0].Module;
        module.ExistingResourcePropertyReferences.Should().ContainKey("acrLoginServer");
        module.ExistingResourcePropertyReferences["acrLoginServer"].ResourceName.Should().Be("acr-secondary");
        module.ExistingResourcePropertyReferences["acrLoginServer"].PropertyPath.Should().Be("properties.loginServer");
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
            Spec = CreateMinimalSpec(),
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
            Spec = CreateMinimalSpec(),
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
    public void Given_ContainerAppWithAcrPullIdentityId_When_Execute_Then_ResolvesAcrManagedIdentityClientIdFromParentModule()
    {
        // Arrange
        var uaiId = Guid.NewGuid();
        var containerAppResource = new ResourceDefinition
        {
            ResourceId = Guid.NewGuid(),
            Name = "my-container-app",
            Type = AzureResourceTypes.ArmTypes.ContainerAppType,
            Properties = new Dictionary<string, string>
            {
                ["acrPullIdentityId"] = uaiId.ToString(),
            },
        };

        var context = new BicepGenerationContext
        {
            Request = new GenerationRequest
            {
                Resources = [containerAppResource],
            },
            ResourceIdToInfo = new Dictionary<Guid, (string Name, string ResourceTypeName)>
            {
                [uaiId] = ("uai-acr-pull", AzureResourceTypes.UserAssignedIdentity),
            },
        };

        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = containerAppResource,
            Module = new GeneratedTypeModule
            {
                Parameters = new Dictionary<string, object>
                {
                    ["acrManagedIdentityClientId"] = string.Empty,
                },
            },
            Spec = CreateMinimalSpec(),
        });

        // Act
        _sut.Execute(context);

        // Assert
        var module = context.WorkItems[0].Module;
        module.ParentModuleOutputReferences.Should().ContainKey("acrManagedIdentityClientId");
        module.ParentModuleOutputReferences["acrManagedIdentityClientId"].Name.Should().Be("uai-acr-pull");
        module.ParentModuleOutputReferences["acrManagedIdentityClientId"].ResourceTypeName.Should().Be(AzureResourceTypes.UserAssignedIdentity);
        module.ParentModuleOutputReferences["acrManagedIdentityClientId"].OutputName.Should().Be("clientId");
        module.Parameters.Should().NotContainKey("acrManagedIdentityClientId");
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

    private static BicepModuleSpec CreateMinimalSpec() => new()
    {
        ModuleName = "test",
        ModuleFolderName = "Test",
        ResourceTypeName = "Test",
        Resource = new BicepResourceDeclaration
        {
            Symbol = "testResource",
            ArmTypeWithApiVersion = "Microsoft.Test/resources@2024-01-01",
        },
    };

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
                Spec = CreateMinimalSpec(),
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
            Spec = CreateMinimalSpec(),
        });
        return context;
    }
}
