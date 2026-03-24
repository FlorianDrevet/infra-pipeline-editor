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
            ModuleFileName = "logAnalyticsWorkspace.bicep",
            ModuleBicepContent = LogAnalyticsWorkspaceModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string LogAnalyticsWorkspaceModuleTemplate = """
        param location string
        param name string
        param sku string = 'PerGB2018'
        param retentionInDays int = 30
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
