using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure App Configuration (<c>Microsoft.AppConfiguration/configurationStores</c>).
/// </summary>
public sealed class AppConfigurationTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.AppConfiguration/configurationStores";

    /// <inheritdoc />
    public string ResourceTypeName => "AppConfiguration";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "appConfiguration",
            ModuleFileName = "appConfiguration.bicep",
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
        """;
}
