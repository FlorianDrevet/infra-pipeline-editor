using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for a <c>Microsoft.ManagedIdentity/userAssignedIdentities</c> resource.
/// Migrated to Builder + IR (Vague 2).
/// </summary>
public sealed class UserAssignedIdentityTypeBicepGenerator : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType => AzureResourceTypes.ArmTypes.UserAssignedIdentity;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.UserAssignedIdentity;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("userAssignedIdentity", "UserAssignedIdentity", AzureResourceTypes.UserAssignedIdentity)
            .Param("location", BicepType.String, description: "Azure region for the User Assigned Identity")
            .Param("name", BicepType.String, description: "Name of the User Assigned Identity")
            .Resource("identity", "Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Output("resourceId", BicepType.String, new BicepRawExpression("identity.id"))
            .Output("principalId", BicepType.String, new BicepRawExpression("identity.properties.principalId"))
            .Output("clientId", BicepType.String, new BicepRawExpression("identity.properties.clientId"))
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "userAssignedIdentity",
            ModuleFileName = "userAssignedIdentity",
            ModuleFolderName = "UserAssignedIdentity",
            ModuleBicepContent = UserAssignedIdentityModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string UserAssignedIdentityModuleTemplate = """
        @description('Azure region for the User Assigned Identity')
        param location string

        @description('Name of the User Assigned Identity')
        param name string

        resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
          name: name
          location: location
        }

        output resourceId string = identity.id
        output principalId string = identity.properties.principalId
        output clientId string = identity.properties.clientId
        """;
}
