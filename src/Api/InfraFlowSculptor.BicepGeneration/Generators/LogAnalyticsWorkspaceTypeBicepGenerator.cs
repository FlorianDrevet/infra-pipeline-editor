using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.GenerationCore.Models;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Log Analytics Workspace (<c>Microsoft.OperationalInsights/workspaces</c>).
/// Migrated to Builder + IR (Vague 2).
/// </summary>
public sealed class LogAnalyticsWorkspaceTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "logAnalyticsWorkspace";
    private const string ModuleFolderName = "LogAnalyticsWorkspace";
    private const string ModuleFileName = "logAnalyticsWorkspace";
    private const string SkuNameTypeName = "SkuName";
    private const string SkuParameterName = "sku";
    private const string RetentionInDaysParameterName = "retentionInDays";
    private const string DailyQuotaGbParameterName = "dailyQuotaGb";
    private const string ResourceSymbol = "logAnalyticsWorkspace";
    private const string LogAnalyticsWorkspaceArmType = "Microsoft.OperationalInsights/workspaces@2023-09-01";
    private const string DefaultSkuName = "PerGB2018";
    private const int DefaultRetentionInDays = 30;
    private const int DefaultDailyQuotaGb = -1;
    private const string WorkspaceCappingPropertyName = "workspaceCapping";
    private const string LogAnalyticsWorkspaceIdOutputName = "logAnalyticsWorkspaceId";
    private const string CustomerIdOutputName = "customerId";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string CustomerIdExpression = ResourceSymbol + ".properties.customerId";
    private const string SkuNameUnion = "'Free' | 'Standalone' | 'PerNode' | 'PerGB2018' | 'Premium' | 'Standard' | 'CapacityReservation' | 'LACluster'";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.LogAnalyticsWorkspaceType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.LogAnalyticsWorkspace;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, AzureResourceTypes.LogAnalyticsWorkspace)
            .Import(TypesImportPath, SkuNameTypeName)
            .Param(LocationParameterName, BicepType.String, description: "Azure region for the Log Analytics workspace")
            .Param(NameParameterName, BicepType.String, description: "Name of the Log Analytics workspace")
            .Param(SkuParameterName, BicepType.Custom(SkuNameTypeName), description: "SKU of the Log Analytics workspace",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Param(RetentionInDaysParameterName, BicepType.Int, description: "Number of days to retain data",
                defaultValue: new BicepIntLiteral(DefaultRetentionInDays))
            .Param(DailyQuotaGbParameterName, BicepType.Int, description: "Daily ingestion quota in GB (-1 for unlimited)",
                defaultValue: new BicepIntLiteral(DefaultDailyQuotaGb))
            .Resource(ResourceSymbol, LogAnalyticsWorkspaceArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(SkuParameterName, sku => sku
                    .Property(NamePropertyName, new BicepReference(SkuParameterName)))
                .Property(RetentionInDaysParameterName, new BicepReference(RetentionInDaysParameterName))
                .Property(WorkspaceCappingPropertyName, capping => capping
                    .Property(DailyQuotaGbParameterName, new BicepReference(DailyQuotaGbParameterName))))
            .Output(LogAnalyticsWorkspaceIdOutputName, BicepType.String,
                new BicepRawExpression(ResourceIdExpression))
            .Output(CustomerIdOutputName, BicepType.String,
                new BicepRawExpression(CustomerIdExpression),
                description: "The customer ID (workspace ID) of the Log Analytics workspace")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU name for the Log Analytics workspace")
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
