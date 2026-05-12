using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Assemblers;

public sealed class BicepAssemblerTests
{
    [Fact]
    public void Given_ContainerRegistryRoleAssignment_When_Assemble_Then_EmitsContainerRegistryRoleAssignmentModuleInContainerRegistryFolder()
    {
        // Arrange
        var roleAssignment = new RoleAssignmentDefinition
        {
            SourceResourceName = "ifs-frontend",
            SourceResourceType = AzureResourceTypes.ArmTypes.ContainerApp,
            SourceResourceTypeName = AzureResourceTypes.ContainerApp,
            SourceResourceGroupName = "ifs",
            TargetResourceName = "ifs",
            TargetResourceType = AzureResourceTypes.ArmTypes.ContainerRegistry,
            TargetResourceTypeName = AzureResourceTypes.ContainerRegistry,
            TargetResourceGroupName = "ifs-core",
            TargetResourceAbbreviation = "cr",
            ManagedIdentityType = "UserAssigned",
            UserAssignedIdentityName = "frontend",
            UserAssignedIdentityResourceId = Guid.NewGuid(),
            RoleDefinitionId = "7f951dda-4ed3-4680-a7ca-43fe172d538d",
            RoleDefinitionName = "AcrPull",
            RoleDefinitionDescription = "Pull images from the registry.",
            ServiceCategory = "containerregistry",
            IsTargetCrossConfig = true,
        };

        var resourceGroups = new[]
        {
            new ResourceGroupDefinition
            {
                Name = "ifs",
                Location = "FranceCentral",
                ResourceAbbreviation = "rg",
            },
        };

        var environments = new[]
        {
            new EnvironmentDefinition
            {
                Name = "Development",
                ShortName = "dev",
                Location = "FranceCentral",
            },
        };

        // Act
        var result = BicepAssembler.Assemble(
            modules: [],
            resourceGroups: resourceGroups,
            environments: environments,
            environmentNames: ["Development"],
            resources: [],
            namingContext: new NamingContext(),
            roleAssignments: [roleAssignment],
            appSettings: [],
            existingResourceReferences: []);

        // Assert
        result.ModuleFiles.Should().ContainKey("modules/ContainerRegistry/containerregistry.roleassignments.module.bicep");
        result.ModuleFiles.Should().NotContainKey("modules/KeyVault/containerregistry.roleassignments.module.bicep");
        result.MainBicep.Should().Contain("./modules/ContainerRegistry/containerregistry.roleassignments.module.bicep");
        result.MainBicep.Should().NotContain("./modules/KeyVault/containerregistry.roleassignments.module.bicep");
        result.MainBicep.Should().Contain("module containerAppIfsFrontendcontainerRegistryIfsRoles");
        result.MainBicep.Should().NotContain("unknown");
    }

    [Fact]
    public void Given_ContainerAppLinkedToExistingContainerRegistry_When_Assemble_Then_ComputesAcrLoginServerInMainBicep()
    {
        // Arrange
        var containerRegistryId = Guid.NewGuid();
        var modules = new[]
        {
            new GeneratedTypeModule
            {
                ModuleName = "containerAppIfsApi",
                ModuleFileName = "containerAppAcrManagedIdentity.module.bicep",
                ModuleFolderName = "ContainerApp",
                ModuleBicepContent = "param location string",
                ResourceGroupName = "ifs",
                LogicalResourceName = "ifs-api",
                ResourceTypeName = AzureResourceTypes.ContainerApp,
                ResourceAbbreviation = "ca",
                Parameters = new Dictionary<string, object>
                {
                    ["acrLoginServer"] = string.Empty,
                    ["acrManagedIdentityClientId"] = string.Empty,
                },
            },
        };

        var resourceGroups = new[]
        {
            new ResourceGroupDefinition
            {
                Name = "ifs",
                Location = "FranceCentral",
                ResourceAbbreviation = "rg",
            },
        };

        var environments = new[]
        {
            new EnvironmentDefinition
            {
                Name = "dev",
                ShortName = "dev",
                Location = "FranceCentral",
            },
        };

        var resources = new[]
        {
            new ResourceDefinition
            {
                ResourceId = Guid.NewGuid(),
                Name = "ifs-api",
                Type = AzureResourceTypes.ArmTypes.ContainerApp,
                ResourceGroupName = "ifs",
                ResourceAbbreviation = "ca",
                Properties = new Dictionary<string, string>
                {
                    ["containerRegistryId"] = containerRegistryId.ToString(),
                    ["acrAuthMode"] = "ManagedIdentity",
                },
            },
        };

        var namingContext = new NamingContext
        {
            ResourceTemplates = new Dictionary<string, string>
            {
                [AzureResourceTypes.ContainerRegistry] = "{name}-{resourceAbbr}{suffix}",
            },
        };

        var existingResourceReferences = new[]
        {
            new ExistingResourceReference
            {
                ResourceName = "ifs",
                ResourceTypeName = AzureResourceTypes.ContainerRegistry,
                ResourceType = AzureResourceTypes.ArmTypes.ContainerRegistry,
                ResourceGroupName = "ifs-core",
                ResourceAbbreviation = "acr",
                SourceConfigName = string.Empty,
            },
        };

        // Act
        var result = BicepAssembler.Assemble(
            modules,
            resourceGroups,
            environments,
            environmentNames: ["dev"],
            resources,
            namingContext,
            roleAssignments: [],
            appSettings: [],
            existingResourceReferences);

        // Assert
        result.MainBicep.Should().Contain("acrLoginServer: containerAppIfsApiAcrLoginServer");
        result.MainBicep.Should().Contain("param containerAppIfsApiAcrLoginServer string");
        result.EnvironmentParameterFiles["main.dev.bicepparam"].Should().Contain("containerAppIfsApiAcrLoginServer");
    }
}