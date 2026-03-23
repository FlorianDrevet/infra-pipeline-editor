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
            ModuleBicepContent = AppConfigurationModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Sku.ToLower(),
            }
        };
    }

    private const string AppConfigurationModuleTemplate = """
        param location string
        param name string
        param sku string
        param softDeleteRetentionInDays int = 7
        param enablePurgeProtection bool = false
        param disableLocalAuth bool = false
        param publicNetworkAccess string = 'Enabled'

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
