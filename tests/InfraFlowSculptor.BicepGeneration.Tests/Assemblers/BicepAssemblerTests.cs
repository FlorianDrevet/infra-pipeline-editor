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
            RoleDefinitionId = "7f951dda-4ed3-4680-a7ca-43fe172d538e",
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
    }
}