using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure App Configuration (<c>Microsoft.AppConfiguration/configurationStores@2023-03-01</c>).
/// </summary>
public sealed class AppConfigurationTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.AppConfiguration;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.AppConfiguration;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("appConfiguration", "AppConfiguration", ResourceTypeName)
            .Import("./types.bicep", "SkuName", "PublicNetworkAccess")
            .Param("location", BicepType.String, "Azure region for the App Configuration store")
            .Param("name", BicepType.String, "Name of the App Configuration store")
            .Param("sku", BicepType.Custom("SkuName"), "SKU of the App Configuration store",
                defaultValue: new BicepStringLiteral("standard"))
            .Param("softDeleteRetentionInDays", BicepType.Int, "Number of days to retain soft-deleted items",
                defaultValue: new BicepIntLiteral(7))
            .Param("enablePurgeProtection", BicepType.Bool, "Whether purge protection is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("disableLocalAuth", BicepType.Bool, "Whether local authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("publicNetworkAccess", BicepType.Custom("PublicNetworkAccess"), "Public network access setting",
                defaultValue: new BicepStringLiteral("Enabled"))
            .Resource("appConfig", "Microsoft.AppConfiguration/configurationStores@2023-03-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("sku", sku => sku
                .Property("name", new BicepReference("sku")))
            .Property("properties", props => props
                .Property("softDeleteRetentionInDays", new BicepReference("softDeleteRetentionInDays"))
                .Property("enablePurgeProtection", new BicepReference("enablePurgeProtection"))
                .Property("disableLocalAuth", new BicepReference("disableLocalAuth"))
                .Property("publicNetworkAccess", new BicepReference("publicNetworkAccess")))
            .Output("id", BicepType.String, new BicepRawExpression("appConfig.id"),
                description: "The resource ID of the App Configuration store")
            .Output("endpoint", BicepType.String, new BicepRawExpression("appConfig.properties.endpoint"),
                description: "The endpoint of the App Configuration store")
            .ExportedType("SkuName",
                new BicepRawExpression("'free' | 'standard'"),
                description: "SKU name for the App Configuration store")
            .ExportedType("PublicNetworkAccess",
                new BicepRawExpression("'Enabled' | 'Disabled'"),
                description: "Public network access setting")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "appConfiguration",
            ModuleFileName = "appConfiguration",
            ModuleFolderName = "AppConfiguration",
            ModuleBicepContent = AppConfigurationModuleTemplate,
            ModuleTypesBicepContent = AppConfigurationTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Sku.ToLower(),
            }
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
