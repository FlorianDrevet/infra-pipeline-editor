using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container Registry (<c>Microsoft.ContainerRegistry/registries</c>).
/// </summary>
public sealed class ContainerRegistryTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerRegistry;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ContainerRegistry;

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "containerRegistry",
            ModuleFileName = "containerRegistry",
            ModuleFolderName = "ContainerRegistry",
            ModuleBicepContent = ContainerRegistryModuleTemplate,
            ModuleTypesBicepContent = ContainerRegistryTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ContainerRegistryTypesTemplate = """
        @export()
        @description('SKU for the Container Registry')
        type SkuName = 'Basic' | 'Standard' | 'Premium'

        @export()
        @description('Public network access setting for the Container Registry')
        type PublicNetworkAccess = 'Enabled' | 'Disabled'
        """;

    private const string ContainerRegistryModuleTemplate = """
        import { SkuName, PublicNetworkAccess } from './types.bicep'

        @description('Azure region for the Container Registry')
        param location string

        @description('Name of the Container Registry')
        param name string

        @description('SKU of the Container Registry')
        param sku SkuName = 'Basic'

        @description('Whether the admin user is enabled')
        param adminUserEnabled bool = false

        @description('Public network access setting')
        param publicNetworkAccess PublicNetworkAccess = 'Enabled'

        @description('Whether zone redundancy is enabled (Premium only)')
        param zoneRedundancy bool = false

        resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
          name: name
          location: location
          sku: {
            name: sku
          }
          properties: {
            adminUserEnabled: adminUserEnabled
            publicNetworkAccess: publicNetworkAccess
            zoneRedundancy: zoneRedundancy ? 'Enabled' : 'Disabled'
          }
        }

        @description('The resource ID of the Container Registry')
        output id string = containerRegistry.id

        @description('The login server of the Container Registry')
        output loginServer string = containerRegistry.properties.loginServer
        """;
}
