using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Application Insights (<c>Microsoft.Insights/components@2020-02-02</c>).
/// </summary>
public sealed class ApplicationInsightsTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ApplicationInsights;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ApplicationInsights;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("applicationInsights", "ApplicationInsights", ResourceTypeName)
            .Import("./types.bicep", "IngestionMode")
            .Param("location", BicepType.String, "Azure region for the Application Insights resource")
            .Param("name", BicepType.String, "Name of the Application Insights resource")
            .Param("logAnalyticsWorkspaceId", BicepType.String, "Resource ID of the Log Analytics workspace")
            .Param("samplingPercentage", BicepType.Int, "Sampling percentage (0-100)",
                defaultValue: new BicepIntLiteral(100))
            .Param("retentionInDays", BicepType.Int, "Number of days to retain data",
                defaultValue: new BicepIntLiteral(90))
            .Param("disableIpMasking", BicepType.Bool, "Whether IP masking is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("disableLocalAuth", BicepType.Bool, "Whether local authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("ingestionMode", BicepType.Custom("IngestionMode"), "Ingestion mode for telemetry data",
                defaultValue: new BicepStringLiteral("LogAnalytics"))
            .Resource("applicationInsights", "Microsoft.Insights/components@2020-02-02")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("kind", new BicepStringLiteral("web"))
            .Property("properties", props => props
                .Property("Application_Type", new BicepStringLiteral("web"))
                .Property("WorkspaceResourceId", new BicepReference("logAnalyticsWorkspaceId"))
                .Property("SamplingPercentage", new BicepReference("samplingPercentage"))
                .Property("RetentionInDays", new BicepReference("retentionInDays"))
                .Property("DisableIpMasking", new BicepReference("disableIpMasking"))
                .Property("DisableLocalAuth", new BicepReference("disableLocalAuth"))
                .Property("IngestionMode", new BicepReference("ingestionMode")))
            .Output("id", BicepType.String, new BicepRawExpression("applicationInsights.id"),
                description: "The resource ID of the Application Insights resource")
            .Output("instrumentationKey", BicepType.String,
                new BicepRawExpression("applicationInsights.properties.InstrumentationKey"),
                description: "The instrumentation key of the Application Insights resource")
            .Output("connectionString", BicepType.String,
                new BicepRawExpression("applicationInsights.properties.ConnectionString"),
                description: "The connection string of the Application Insights resource")
            .ExportedType("IngestionMode",
                new BicepRawExpression("'ApplicationInsights' | 'ApplicationInsightsWithDiagnosticSettings' | 'LogAnalytics'"),
                description: "Ingestion mode for Application Insights")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "applicationInsights",
            ModuleFileName = "applicationInsights",
            ModuleFolderName = "ApplicationInsights",
            ModuleBicepContent = ApplicationInsightsModuleTemplate,
            ModuleTypesBicepContent = ApplicationInsightsTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ApplicationInsightsTypesTemplate = """
        @export()
        @description('Ingestion mode for Application Insights')
        type IngestionMode = 'ApplicationInsights' | 'ApplicationInsightsWithDiagnosticSettings' | 'LogAnalytics'
        """;

    private const string ApplicationInsightsModuleTemplate = """
        import { IngestionMode } from './types.bicep'

        @description('Azure region for the Application Insights resource')
        param location string

        @description('Name of the Application Insights resource')
        param name string

        @description('Resource ID of the Log Analytics workspace')
        param logAnalyticsWorkspaceId string

        @description('Sampling percentage (0-100)')
        param samplingPercentage int = 100

        @description('Number of days to retain data')
        param retentionInDays int = 90

        @description('Whether IP masking is disabled')
        param disableIpMasking bool = false

        @description('Whether local authentication is disabled')
        param disableLocalAuth bool = false

        @description('Ingestion mode for telemetry data')
        param ingestionMode IngestionMode = 'LogAnalytics'

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

        @description('The resource ID of the Application Insights resource')
        output id string = applicationInsights.id

        @description('The instrumentation key of the Application Insights resource')
        output instrumentationKey string = applicationInsights.properties.InstrumentationKey

        @description('The connection string of the Application Insights resource')
        output connectionString string = applicationInsights.properties.ConnectionString
        """;
}
