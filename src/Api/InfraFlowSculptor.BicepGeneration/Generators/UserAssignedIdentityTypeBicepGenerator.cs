using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for a <c>Microsoft.ManagedIdentity/userAssignedIdentities</c> resource.
/// </summary>
public sealed class UserAssignedIdentityTypeBicepGenerator : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType => AzureResourceTypes.ArmTypes.UserAssignedIdentity;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.UserAssignedIdentity;

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

    private static readonly string UserAssignedIdentityModuleTemplate = $$"""
        @description('Azure region for the User Assigned Identity')
        param location string

        @description('Name of the User Assigned Identity')
        param name string

        resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@{{AzureResourceTypes.ApiVersions.UserAssignedIdentity}}' = {
          name: name
          location: location
        }

        output resourceId string = identity.id
        output principalId string = identity.properties.principalId
        output clientId string = identity.properties.clientId
        """;
}
