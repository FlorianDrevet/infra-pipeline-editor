using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure App Configuration (<c>Microsoft.AppConfiguration/configurationStores@2023-03-01</c>).
/// </summary>
public sealed class AppConfigurationTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "appConfiguration";
    private const string ModuleFolderName = "AppConfiguration";
    private const string SkuNameTypeName = "SkuName";
    private const string PublicNetworkAccessTypeName = "PublicNetworkAccess";
    private const string SkuParameterName = "sku";
    private const string SoftDeleteRetentionInDaysParameterName = "softDeleteRetentionInDays";
    private const string EnablePurgeProtectionParameterName = "enablePurgeProtection";
    private const string DisableLocalAuthParameterName = "disableLocalAuth";
    private const string PublicNetworkAccessParameterName = "publicNetworkAccess";
    private const string ResourceSymbol = "appConfig";
    private const string AppConfigurationArmType = "Microsoft.AppConfiguration/configurationStores@2023-03-01";
    private const string DefaultSkuName = "standard";
    private const int DefaultSoftDeleteRetentionInDays = 7;
    private const string PublicNetworkAccessEnabledValue = "Enabled";
    private const string SkuNameUnion = "'free' | 'standard'";
    private const string PublicNetworkAccessUnion = "'Enabled' | 'Disabled'";
    private const string EndpointOutputName = "endpoint";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string EndpointExpression = ResourceSymbol + ".properties.endpoint";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.AppConfigurationType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.AppConfiguration;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SkuNameTypeName, PublicNetworkAccessTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the App Configuration store")
            .Param(NameParameterName, BicepType.String, "Name of the App Configuration store")
            .Param(SkuParameterName, BicepType.Custom(SkuNameTypeName), "SKU of the App Configuration store",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Param(SoftDeleteRetentionInDaysParameterName, BicepType.Int, "Number of days to retain soft-deleted items",
                defaultValue: new BicepIntLiteral(DefaultSoftDeleteRetentionInDays))
            .Param(EnablePurgeProtectionParameterName, BicepType.Bool, "Whether purge protection is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(DisableLocalAuthParameterName, BicepType.Bool, "Whether local authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(PublicNetworkAccessParameterName, BicepType.Custom(PublicNetworkAccessTypeName), "Public network access setting",
                defaultValue: new BicepStringLiteral(PublicNetworkAccessEnabledValue))
            .Resource(ResourceSymbol, AppConfigurationArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(SkuParameterName, sku => sku
                .Property(NamePropertyName, new BicepReference(SkuParameterName)))
            .Property(PropertiesPropertyName, props => props
                .Property(SoftDeleteRetentionInDaysParameterName, new BicepReference(SoftDeleteRetentionInDaysParameterName))
                .Property(EnablePurgeProtectionParameterName, new BicepReference(EnablePurgeProtectionParameterName))
                .Property(DisableLocalAuthParameterName, new BicepReference(DisableLocalAuthParameterName))
                .Property(PublicNetworkAccessParameterName, new BicepReference(PublicNetworkAccessParameterName)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the App Configuration store")
            .Output(EndpointOutputName, BicepType.String, new BicepRawExpression(EndpointExpression),
                description: "The endpoint of the App Configuration store")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU name for the App Configuration store")
            .ExportedType(PublicNetworkAccessTypeName,
                new BicepRawExpression(PublicNetworkAccessUnion),
                description: "Public network access setting")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleName,
            ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = AppConfigurationModuleTemplate,
            ModuleTypesBicepContent = AppConfigurationTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = BicepParameterModelConverter.ToDictionary(new AppConfigurationParameters
            {
                Sku = resource.Sku.ToLower(),
            })
        };
    }

    private const string AppConfigurationTypesTemplate = """
        @export()
        @description('SKU name for the App Configuration store')
        type SkuName = 'free' | 'standard'

        @export()
        @description('Public network access setting')
        type PublicNetworkAccess = 'Enabled' | 'Disabled'
        """;

    private const string AppConfigurationModuleTemplate = """
        import { SkuName, PublicNetworkAccess } from './types.bicep'

        @description('Azure region for the App Configuration store')
        param location string

        @description('Name of the App Configuration store')
        param name string

        @description('SKU of the App Configuration store')
        param sku SkuName = 'standard'

        @description('Number of days to retain soft-deleted items')
        param softDeleteRetentionInDays int = 7

        @description('Whether purge protection is enabled')
        param enablePurgeProtection bool = false

        @description('Whether local authentication is disabled')
        param disableLocalAuth bool = false

        @description('Public network access setting')
        param publicNetworkAccess PublicNetworkAccess = 'Enabled'

        resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
          name: name
          location: location
          sku: {
            name: sku
          }
          properties: {
            softDeleteRetentionInDays: softDeleteRetentionInDays
            enablePurgeProtection: enablePurgeProtection
            disableLocalAuth: disableLocalAuth
            publicNetworkAccess: publicNetworkAccess
          }
        }

        @description('The resource ID of the App Configuration store')
        output id string = appConfig.id

        @description('The endpoint of the App Configuration store')
        output endpoint string = appConfig.properties.endpoint
        """;
}
