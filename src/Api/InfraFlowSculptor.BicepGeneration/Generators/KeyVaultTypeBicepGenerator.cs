using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class KeyVaultTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => AzureResourceTypes.ArmTypes.KeyVault;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.KeyVault;

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var enableRbac = resource.Properties.GetValueOrDefault("enableRbacAuthorization", "true");
        var enabledForDeployment = resource.Properties.GetValueOrDefault("enabledForDeployment", "false");
        var enabledForDiskEncryption = resource.Properties.GetValueOrDefault("enabledForDiskEncryption", "false");
        var enabledForTemplateDeployment = resource.Properties.GetValueOrDefault("enabledForTemplateDeployment", "false");
        var enablePurgeProtection = resource.Properties.GetValueOrDefault("enablePurgeProtection", "true");
        var enableSoftDelete = resource.Properties.GetValueOrDefault("enableSoftDelete", "true");

        return new GeneratedTypeModule
        {
            ModuleName = "keyVault",
            ModuleFileName = "keyVault",
            ModuleFolderName = "KeyVault",
            ModuleBicepContent = BuildModuleTemplate(
                enableRbac, enabledForDeployment, enabledForDiskEncryption,
                enabledForTemplateDeployment, enablePurgeProtection, enableSoftDelete),
            ModuleTypesBicepContent = KeyVaultTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Sku.ToLower(),
            }
        };
    }

    private static string BuildModuleTemplate(
        string enableRbac,
        string enabledForDeployment,
        string enabledForDiskEncryption,
        string enabledForTemplateDeployment,
        string enablePurgeProtection,
        string enableSoftDelete)
    {
        return $$"""
            import { SkuName } from './types.bicep'

            @description('Azure region for the Key Vault')
            param location string

            @description('Name of the Key Vault')
            param name string

            @description('SKU of the Key Vault')
            param sku SkuName = 'standard'

            resource kv 'Microsoft.KeyVault/vaults@{{AzureResourceTypes.ApiVersions.KeyVault}}' = {
              name: name
              location: location
              properties: {
                sku: {
                  family: 'A'
                  name: sku
                }
                tenantId: subscription().tenantId
                enableRbacAuthorization: {{enableRbac}}
                enabledForDeployment: {{enabledForDeployment}}
                enabledForDiskEncryption: {{enabledForDiskEncryption}}
                enabledForTemplateDeployment: {{enabledForTemplateDeployment}}
                enablePurgeProtection: {{enablePurgeProtection}}
                enableSoftDelete: {{enableSoftDelete}}
              }
            }

            @description('The resource ID of the Key Vault')
            output id string = kv.id

            @description('The name of the Key Vault')
            output name string = kv.name

            @description('The URI of the Key Vault')
            output vaultUri string = kv.properties.vaultUri
            """;
    }

    private const string KeyVaultTypesTemplate = """
        @export()
        @description('SKU name for the Key Vault')
        type SkuName = 'premium' | 'standard'
        """;
}
