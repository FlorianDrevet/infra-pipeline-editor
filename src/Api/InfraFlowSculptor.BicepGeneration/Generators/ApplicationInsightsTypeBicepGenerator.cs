using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Application Insights (<c>Microsoft.Insights/components</c>).
/// </summary>
public sealed class ApplicationInsightsTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.Insights/components";

    /// <inheritdoc />
    public string ResourceTypeName => "ApplicationInsights";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "applicationInsights",
            ModuleFileName = "applicationInsights.bicep",
            ModuleBicepContent = ApplicationInsightsModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ApplicationInsightsModuleTemplate = """
        param location string
        param name string
        param logAnalyticsWorkspaceId string
        param samplingPercentage int = 100
        param retentionInDays int = 90
        param disableIpMasking bool = false
        param disableLocalAuth bool = false
        param ingestionMode string = 'LogAnalytics'

        resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
          name: name
          location: location
          kind: 'web'
          properties: {
            Application_Type: 'web'
            WorkspaceResourceId: logAnalyticsWorkspaceId
            SamplingPercentage: samplingPercentage
            RetentionInDays: retentionInDays
            DisableIpMasking: disableIpMasking
            DisableLocalAuth: disableLocalAuth
            IngestionMode: ingestionMode
          }
        }
        """;
}
