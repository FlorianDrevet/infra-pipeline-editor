using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Key Vault (<c>Microsoft.KeyVault/vaults@2023-07-01</c>).
/// </summary>
public sealed class KeyVaultTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "keyVault";
    private const string ModuleFolderName = "KeyVault";
    private const string SkuNameTypeName = "SkuName";
    private const string SkuParameterName = "sku";
    private const string ResourceSymbol = "kv";
    private const string KeyVaultArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.KeyVaultArmType;
    private const string DefaultSkuName = "standard";
    private const string SkuFamilyValue = "A";
    private const string TenantIdExpression = "subscription().tenantId";
    private const string EnableRbacAuthorizationPropertyName = "enableRbacAuthorization";
    private const string EnabledForDeploymentPropertyName = "enabledForDeployment";
    private const string EnabledForDiskEncryptionPropertyName = "enabledForDiskEncryption";
    private const string EnabledForTemplateDeploymentPropertyName = "enabledForTemplateDeployment";
    private const string EnablePurgeProtectionPropertyName = "enablePurgeProtection";
    private const string EnableSoftDeletePropertyName = "enableSoftDelete";
    private const string SkuNameUnion = "'premium' | 'standard'";
    private const string FamilyPropertyName = "family";
    private const string TenantIdPropertyName = "tenantId";
    private const string VaultUriOutputName = "vaultUri";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string ResourceNameExpression = ResourceSymbol + ".name";
    private const string VaultUriExpression = ResourceSymbol + ".properties.vaultUri";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.KeyVaultType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.KeyVault;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        var enableRbac = bool.Parse(
            resource.Properties.GetValueOrDefault(EnableRbacAuthorizationPropertyName, BooleanTrueString)!);
        var enabledForDeployment = bool.Parse(
            resource.Properties.GetValueOrDefault(EnabledForDeploymentPropertyName, BooleanFalseString)!);
        var enabledForDiskEncryption = bool.Parse(
            resource.Properties.GetValueOrDefault(EnabledForDiskEncryptionPropertyName, BooleanFalseString)!);
        var enabledForTemplateDeployment = bool.Parse(
            resource.Properties.GetValueOrDefault(EnabledForTemplateDeploymentPropertyName, BooleanFalseString)!);
        var enablePurgeProtection = bool.Parse(
            resource.Properties.GetValueOrDefault(EnablePurgeProtectionPropertyName, BooleanTrueString)!);
        var enableSoftDelete = bool.Parse(
            resource.Properties.GetValueOrDefault(EnableSoftDeletePropertyName, BooleanTrueString)!);

        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SkuNameTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Key Vault")
            .Param(NameParameterName, BicepType.String, "Name of the Key Vault")
            .Param(SkuParameterName, BicepType.Custom(SkuNameTypeName), "SKU of the Key Vault",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Resource(ResourceSymbol, KeyVaultArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(SkuParameterName, sku => sku
                    .Property(FamilyPropertyName, new BicepStringLiteral(SkuFamilyValue))
                    .Property(NamePropertyName, new BicepReference(SkuParameterName)))
                .Property(TenantIdPropertyName, new BicepRawExpression(TenantIdExpression))
                .Property(EnableRbacAuthorizationPropertyName, new BicepBoolLiteral(enableRbac))
                .Property(EnabledForDeploymentPropertyName, new BicepBoolLiteral(enabledForDeployment))
                .Property(EnabledForDiskEncryptionPropertyName, new BicepBoolLiteral(enabledForDiskEncryption))
                .Property(EnabledForTemplateDeploymentPropertyName, new BicepBoolLiteral(enabledForTemplateDeployment))
                .Property(EnablePurgeProtectionPropertyName, new BicepBoolLiteral(enablePurgeProtection))
                .Property(EnableSoftDeletePropertyName, new BicepBoolLiteral(enableSoftDelete)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Key Vault")
            .Output(NameParameterName, BicepType.String, new BicepRawExpression(ResourceNameExpression),
                description: "The name of the Key Vault")
            .Output(VaultUriOutputName, BicepType.String, new BicepRawExpression(VaultUriExpression),
                description: "The URI of the Key Vault")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU name for the Key Vault")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var enableRbac = resource.Properties.GetValueOrDefault(EnableRbacAuthorizationPropertyName, BooleanTrueString);
        var enabledForDeployment = resource.Properties.GetValueOrDefault(EnabledForDeploymentPropertyName, BooleanFalseString);
        var enabledForDiskEncryption = resource.Properties.GetValueOrDefault(EnabledForDiskEncryptionPropertyName, BooleanFalseString);
        var enabledForTemplateDeployment = resource.Properties.GetValueOrDefault(EnabledForTemplateDeploymentPropertyName, BooleanFalseString);
        var enablePurgeProtection = resource.Properties.GetValueOrDefault(EnablePurgeProtectionPropertyName, BooleanTrueString);
        var enableSoftDelete = resource.Properties.GetValueOrDefault(EnableSoftDeletePropertyName, BooleanTrueString);

        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleName,
            ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = BuildModuleTemplate(
                enableRbac, enabledForDeployment, enabledForDiskEncryption,
                enabledForTemplateDeployment, enablePurgeProtection, enableSoftDelete),
            ModuleTypesBicepContent = KeyVaultTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = BicepParameterModelConverter.ToDictionary(new KeyVaultParameters
            {
                Sku = resource.Sku.ToLower(),
            })
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

            resource kv '{{KeyVaultArmType}}' = {
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
