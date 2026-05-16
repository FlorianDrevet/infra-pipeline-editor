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
            SourceResourceType = AzureResourceTypes.ArmTypes.ContainerAppType,
            SourceResourceTypeName = AzureResourceTypes.ContainerApp,
            SourceResourceGroupName = "ifs",
            TargetResourceName = "ifs",
            TargetResourceType = AzureResourceTypes.ArmTypes.ContainerRegistryType,
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
            new GenerationRequest
            {
                ResourceGroups = resourceGroups,
                Environments = environments,
                EnvironmentNames = ["Development"],
                Resources = [],
                NamingContext = new NamingContext(),
                RoleAssignments = [roleAssignment],
                AppSettings = [],
                ExistingResourceReferences = [],
            });

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
                    ["acrManagedIdentityClientId"] = string.Empty,
                },
                ExistingResourcePropertyReferences = new Dictionary<string, (string ResourceName, string PropertyPath)>
                {
                    ["acrLoginServer"] = ("infraflowsculptor", "properties.loginServer"),
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
                ResourceName = "infraflowsculptor",
                ResourceTypeName = AzureResourceTypes.ContainerRegistry,
                ResourceType = AzureResourceTypes.ArmTypes.ContainerRegistryType,
                ResourceGroupName = "ifs-core",
                ResourceAbbreviation = "acr",
                SourceConfigName = string.Empty,
            },
        };

        // Act
        var result = BicepAssembler.Assemble(
            modules,
            new GenerationRequest
            {
                ResourceGroups = resourceGroups,
                Environments = environments,
                EnvironmentNames = ["dev"],
                Resources = [],
                NamingContext = namingContext,
                RoleAssignments = [],
                AppSettings = [],
                ExistingResourceReferences = existingResourceReferences,
            });

        // Assert
        result.MainBicep.Should().Contain("acrLoginServer: existing_infraflowsculptor.properties.loginServer");
        result.MainBicep.Should().NotContain("param containerAppIfsApiAcrLoginServer string");
        result.EnvironmentParameterFiles["main.dev.bicepparam"].Should().NotContain("containerAppIfsApiAcrLoginServer");
    }

    [Fact]
    public void Given_CrossConfigurationExistingResources_When_Assemble_Then_EmitsAsciiSectionComments()
    {
        // Arrange
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

        var existingResourceReferences = new[]
        {
            new ExistingResourceReference
            {
                ResourceName = "infraflowsculptor",
                ResourceTypeName = AzureResourceTypes.ContainerRegistry,
                ResourceType = AzureResourceTypes.ArmTypes.ContainerRegistryType,
                ResourceGroupName = "ifs-core",
                ResourceAbbreviation = "acr",
                SourceConfigName = "shared",
            },
        };

        // Act
        var result = BicepAssembler.Assemble(
            modules: [],
            new GenerationRequest
            {
                ResourceGroups = resourceGroups,
                Environments = environments,
                EnvironmentNames = ["Development"],
                Resources = [],
                NamingContext = new NamingContext(),
                RoleAssignments = [],
                AppSettings = [],
                ExistingResourceReferences = existingResourceReferences,
            });

        // Assert
        result.MainBicep.Should().Contain("// -- Cross-configuration existing resource groups");
        result.MainBicep.Should().Contain("// -- Cross-configuration existing resources");
        result.MainBicep.Should().NotContain("â");
    }
}