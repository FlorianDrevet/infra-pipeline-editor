using FluentAssertions;
using InfraFlowSculptor.BicepGeneration.Assemblers;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Tests.Assemblers;

public sealed class ConstantsBicepAssemblerTests
{
    [Fact]
    public void Given_MixedRoleNames_When_Generate_Then_FormatsObjectKeysAccordingToBicepIdentifierRules()
    {
        // Arrange
        var roleAssignments = new[]
        {
            new RoleAssignmentDefinition
            {
                RoleDefinitionId = "7f951dda-4ed3-4680-a7ca-43fe172d538d",
                RoleDefinitionName = "AcrPull",
                RoleDefinitionDescription = "Allows pull of images from an Azure Container Registry.",
                ServiceCategory = "containerregistry",
            },
            new RoleAssignmentDefinition
            {
                RoleDefinitionId = "4633458b-17de-408a-b874-0445c86b69e6",
                RoleDefinitionName = "Key Vault Secrets User",
                RoleDefinitionDescription = "Read secret contents including the secret portion of a certificate with private key.",
                ServiceCategory = "keyvault",
            },
        };

        // Act
        var result = ConstantsBicepAssembler.Generate(roleAssignments);

        // Assert
        result.Should().Contain("AcrPull: {");
        result.Should().Contain("'Key Vault Secrets User': {");
    }
}