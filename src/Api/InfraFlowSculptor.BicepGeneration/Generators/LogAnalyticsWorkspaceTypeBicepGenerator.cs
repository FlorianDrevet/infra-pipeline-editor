using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Log Analytics Workspace (<c>Microsoft.OperationalInsights/workspaces</c>).
/// </summary>
public sealed class LogAnalyticsWorkspaceTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.OperationalInsights/workspaces";

    /// <inheritdoc />
    public string ResourceTypeName => "LogAnalyticsWorkspace";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "logAnalyticsWorkspace",
            ModuleFileName = "logAnalyticsWorkspace",
            ModuleFolderName = "LogAnalyticsWorkspace",
            ModuleBicepContent = LogAnalyticsWorkspaceModuleTemplate,
            ModuleTypesBicepContent = LogAnalyticsWorkspaceTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string LogAnalyticsWorkspaceTypesTemplate = """
        @export()
        @description('SKU name for the Log Analytics workspace')
        type SkuName = 'Free' | 'Standalone' | 'PerNode' | 'PerGB2018' | 'Premium' | 'Standard' | 'CapacityReservation' | 'LACluster'
        """;

    private const string LogAnalyticsWorkspaceModuleTemplate = """
        import { SkuName } from './types.bicep'

        @description('Azure region for the Log Analytics workspace')
        param location string

        @description('Name of the Log Analytics workspace')
        param name string

        @description('SKU of the Log Analytics workspace')
        param sku SkuName = 'PerGB2018'

        @description('Number of days to retain data')
        param retentionInDays int = 30

        @description('Daily ingestion quota in GB (-1 for unlimited)')
        param dailyQuotaGb int = -1

        resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
          name: name
          location: location
          properties: {
            sku: {
              name: sku
            }
            retentionInDays: retentionInDays
            workspaceCapping: {
              dailyQuotaGb: dailyQuotaGb
            }
          }
        }

        output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
        """;
}
