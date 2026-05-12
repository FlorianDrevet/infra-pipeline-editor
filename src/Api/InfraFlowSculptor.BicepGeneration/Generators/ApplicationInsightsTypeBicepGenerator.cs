using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Application Insights (<c>Microsoft.Insights/components@2020-02-02</c>).
/// </summary>
public sealed class ApplicationInsightsTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "applicationInsights";
    private const string ModuleFolderName = "ApplicationInsights";
    private const string ModuleFileName = "applicationInsights";
    private const string IngestionModeTypeName = "IngestionMode";
    private const string LogAnalyticsWorkspaceIdParameterName = "logAnalyticsWorkspaceId";
    private const string SamplingPercentageParameterName = "samplingPercentage";
    private const string RetentionInDaysParameterName = "retentionInDays";
    private const string DisableIpMaskingParameterName = "disableIpMasking";
    private const string DisableLocalAuthParameterName = "disableLocalAuth";
    private const string IngestionModeParameterName = "ingestionMode";
    private const string ApplicationInsightsArmType = "Microsoft.Insights/components@2020-02-02";
    private const string ApplicationTypePropertyName = "Application_Type";
    private const string WorkspaceResourceIdPropertyName = "WorkspaceResourceId";
    private const string SamplingPercentagePropertyName = "SamplingPercentage";
    private const string RetentionInDaysPropertyName = "RetentionInDays";
    private const string DisableIpMaskingPropertyName = "DisableIpMasking";
    private const string DisableLocalAuthPropertyName = "DisableLocalAuth";
    private const string IngestionModePropertyName = "IngestionMode";
    private const string WebKindValue = "web";
    private const int DefaultSamplingPercentage = 100;
    private const int DefaultRetentionInDays = 90;
    private const string DefaultIngestionMode = "LogAnalytics";
    private const string InstrumentationKeyOutputName = "instrumentationKey";
    private const string ConnectionStringOutputName = "connectionString";
    private const string ApplicationInsightsIdExpression = ModuleName + ".id";
    private const string InstrumentationKeyExpression = ModuleName + ".properties.InstrumentationKey";
    private const string ConnectionStringExpression = ModuleName + ".properties.ConnectionString";
    private const string IngestionModeUnionExpression = "'ApplicationInsights' | 'ApplicationInsightsWithDiagnosticSettings' | 'LogAnalytics'";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ApplicationInsightsType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ApplicationInsights;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, IngestionModeTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Application Insights resource")
            .Param(NameParameterName, BicepType.String, "Name of the Application Insights resource")
            .Param(LogAnalyticsWorkspaceIdParameterName, BicepType.String, "Resource ID of the Log Analytics workspace")
            .Param(SamplingPercentageParameterName, BicepType.Int, "Sampling percentage (0-100)",
                defaultValue: new BicepIntLiteral(DefaultSamplingPercentage))
            .Param(RetentionInDaysParameterName, BicepType.Int, "Number of days to retain data",
                defaultValue: new BicepIntLiteral(DefaultRetentionInDays))
            .Param(DisableIpMaskingParameterName, BicepType.Bool, "Whether IP masking is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(DisableLocalAuthParameterName, BicepType.Bool, "Whether local authentication is disabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(IngestionModeParameterName, BicepType.Custom(IngestionModeTypeName), "Ingestion mode for telemetry data",
                defaultValue: new BicepStringLiteral(DefaultIngestionMode))
            .Resource(ModuleName, ApplicationInsightsArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(KindPropertyName, new BicepStringLiteral(WebKindValue))
            .Property(PropertiesPropertyName, props => props
                .Property(ApplicationTypePropertyName, new BicepStringLiteral(WebKindValue))
                .Property(WorkspaceResourceIdPropertyName, new BicepReference(LogAnalyticsWorkspaceIdParameterName))
                .Property(SamplingPercentagePropertyName, new BicepReference(SamplingPercentageParameterName))
                .Property(RetentionInDaysPropertyName, new BicepReference(RetentionInDaysParameterName))
                .Property(DisableIpMaskingPropertyName, new BicepReference(DisableIpMaskingParameterName))
                .Property(DisableLocalAuthPropertyName, new BicepReference(DisableLocalAuthParameterName))
                .Property(IngestionModePropertyName, new BicepReference(IngestionModeParameterName)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ApplicationInsightsIdExpression),
                description: "The resource ID of the Application Insights resource")
            .Output(InstrumentationKeyOutputName, BicepType.String,
                new BicepRawExpression(InstrumentationKeyExpression),
                description: "The instrumentation key of the Application Insights resource")
            .Output(ConnectionStringOutputName, BicepType.String,
                new BicepRawExpression(ConnectionStringExpression),
                description: "The connection string of the Application Insights resource")
            .ExportedType(IngestionModeTypeName,
                new BicepRawExpression(IngestionModeUnionExpression),
                description: "Ingestion mode for Application Insights")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleFileName,
            ModuleFolderName = ModuleFolderName,
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
