using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class KeyVaultTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.KeyVault/vaults";

    /// <inheritdoc />
    public string ResourceTypeName => "KeyVault";

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "keyVault",
            ModuleFileName = "keyVault.bicep",
            ModuleFolderName = "KeyVault",
            ModuleBicepContent = KeyVaultModuleTemplate,
            ModuleTypesBicepContent = KeyVaultTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Sku.ToLower(),
            }
        };
    }

    private const string KeyVaultTypesTemplate = """
        @export()
        @description('SKU name for the Key Vault')
        type SkuName = 'premium' | 'standard'
        """;

    private const string KeyVaultModuleTemplate = """
        import { SkuName } from './types.bicep'

        @description('Azure region for the Key Vault')
        param location string

        @description('Name of the Key Vault')
        param name string

        @description('SKU of the Key Vault')
        param sku SkuName = 'standard'

        resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
          name: name
          location: location
          properties: {
            sku: {
              family: 'A'
              name: sku
            }
            tenantId: subscription().tenantId
            enabledForDeployment: true
            enabledForTemplateDeployment: true
            enableSoftDelete: true
          }
        }
        """;
}
