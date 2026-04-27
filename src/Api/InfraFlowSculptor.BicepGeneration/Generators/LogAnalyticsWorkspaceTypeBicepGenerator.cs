using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Log Analytics Workspace (<c>Microsoft.OperationalInsights/workspaces</c>).
/// Migrated to Builder + IR (Vague 2).
/// </summary>
public sealed class LogAnalyticsWorkspaceTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.LogAnalyticsWorkspace;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.LogAnalyticsWorkspace;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("logAnalyticsWorkspace", "LogAnalyticsWorkspace", AzureResourceTypes.LogAnalyticsWorkspace)
            .Import("./types.bicep", "SkuName")
            .Param("location", BicepType.String, description: "Azure region for the Log Analytics workspace")
            .Param("name", BicepType.String, description: "Name of the Log Analytics workspace")
            .Param("sku", BicepType.Custom("SkuName"), description: "SKU of the Log Analytics workspace",
                defaultValue: new BicepStringLiteral("PerGB2018"))
            .Param("retentionInDays", BicepType.Int, description: "Number of days to retain data",
                defaultValue: new BicepIntLiteral(30))
            .Param("dailyQuotaGb", BicepType.Int, description: "Daily ingestion quota in GB (-1 for unlimited)",
                defaultValue: new BicepIntLiteral(-1))
            .Resource("logAnalyticsWorkspace", "Microsoft.OperationalInsights/workspaces@2023-09-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("properties", props => props
                .Property("sku", sku => sku
                    .Property("name", new BicepReference("sku")))
                .Property("retentionInDays", new BicepReference("retentionInDays"))
                .Property("workspaceCapping", capping => capping
                    .Property("dailyQuotaGb", new BicepReference("dailyQuotaGb"))))
            .Output("logAnalyticsWorkspaceId", BicepType.String,
                new BicepRawExpression("logAnalyticsWorkspace.id"))
            .Output("customerId", BicepType.String,
                new BicepRawExpression("logAnalyticsWorkspace.properties.customerId"),
                description: "The customer ID (workspace ID) of the Log Analytics workspace")
            .ExportedType("SkuName",
                new BicepRawExpression("'Free' | 'Standalone' | 'PerNode' | 'PerGB2018' | 'Premium' | 'Standard' | 'CapacityReservation' | 'LACluster'"),
                description: "SKU name for the Log Analytics workspace")
            .Build();
    }

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

        @description('The customer ID (workspace ID) of the Log Analytics workspace')
        output customerId string = logAnalyticsWorkspace.properties.customerId
        """;
}
