using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Key Vault (<c>Microsoft.KeyVault/vaults@2023-07-01</c>).
/// </summary>
public sealed class KeyVaultTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.KeyVault;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.KeyVault;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        var enableRbac = bool.Parse(
            resource.Properties.GetValueOrDefault("enableRbacAuthorization", "true")!);
        var enabledForDeployment = bool.Parse(
            resource.Properties.GetValueOrDefault("enabledForDeployment", "false")!);
        var enabledForDiskEncryption = bool.Parse(
            resource.Properties.GetValueOrDefault("enabledForDiskEncryption", "false")!);
        var enabledForTemplateDeployment = bool.Parse(
            resource.Properties.GetValueOrDefault("enabledForTemplateDeployment", "false")!);
        var enablePurgeProtection = bool.Parse(
            resource.Properties.GetValueOrDefault("enablePurgeProtection", "true")!);
        var enableSoftDelete = bool.Parse(
            resource.Properties.GetValueOrDefault("enableSoftDelete", "true")!);

        return new BicepModuleBuilder()
            .Module("keyVault", "KeyVault", ResourceTypeName)
            .Import("./types.bicep", "SkuName")
            .Param("location", BicepType.String, "Azure region for the Key Vault")
            .Param("name", BicepType.String, "Name of the Key Vault")
            .Param("sku", BicepType.Custom("SkuName"), "SKU of the Key Vault",
                defaultValue: new BicepStringLiteral("standard"))
            .Resource("kv", "Microsoft.KeyVault/vaults@2023-07-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("properties", props => props
                .Property("sku", sku => sku
                    .Property("family", new BicepStringLiteral("A"))
                    .Property("name", new BicepReference("sku")))
                .Property("tenantId", new BicepRawExpression("subscription().tenantId"))
                .Property("enableRbacAuthorization", new BicepBoolLiteral(enableRbac))
                .Property("enabledForDeployment", new BicepBoolLiteral(enabledForDeployment))
                .Property("enabledForDiskEncryption", new BicepBoolLiteral(enabledForDiskEncryption))
                .Property("enabledForTemplateDeployment", new BicepBoolLiteral(enabledForTemplateDeployment))
                .Property("enablePurgeProtection", new BicepBoolLiteral(enablePurgeProtection))
                .Property("enableSoftDelete", new BicepBoolLiteral(enableSoftDelete)))
            .Output("id", BicepType.String, new BicepRawExpression("kv.id"),
                description: "The resource ID of the Key Vault")
            .Output("name", BicepType.String, new BicepRawExpression("kv.name"),
                description: "The name of the Key Vault")
            .Output("vaultUri", BicepType.String, new BicepRawExpression("kv.properties.vaultUri"),
                description: "The URI of the Key Vault")
            .ExportedType("SkuName",
                new BicepRawExpression("'premium' | 'standard'"),
                description: "SKU name for the Key Vault")
            .Build();
    }

    /// <inheritdoc />
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

            resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
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
