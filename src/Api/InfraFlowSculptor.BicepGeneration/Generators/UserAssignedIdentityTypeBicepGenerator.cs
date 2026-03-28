using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for a <c>Microsoft.ManagedIdentity/userAssignedIdentities</c> resource.
/// </summary>
public sealed class UserAssignedIdentityTypeBicepGenerator : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType => "Microsoft.ManagedIdentity/userAssignedIdentities";

    /// <inheritdoc />
    public string ResourceTypeName => "UserAssignedIdentity";

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
