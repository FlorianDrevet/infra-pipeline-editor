using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App Environment (<c>Microsoft.App/managedEnvironments@2024-03-01</c>).
/// </summary>
public sealed class ContainerAppEnvironmentTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "containerAppEnvironment";
    private const string ModuleFolderName = "ContainerAppEnvironment";
    private const string ModuleFileName = "containerAppEnvironment";
    private const string WorkloadProfileTypeName = "WorkloadProfileType";
    private const string WorkloadProfileTypeParameterName = "workloadProfileType";
    private const string InternalLoadBalancerEnabledParameterName = "internalLoadBalancerEnabled";
    private const string ZoneRedundancyEnabledParameterName = "zoneRedundancyEnabled";
    private const string LogAnalyticsWorkspaceIdParameterName = "logAnalyticsWorkspaceId";
    private const string ResourceSymbol = "containerAppEnv";
    private const string ContainerAppEnvironmentArmType = "Microsoft.App/managedEnvironments@2024-03-01";
    private const string DiagnosticSettingsResourceName = "diagnosticSettings";
    private const string DiagnosticSettingsArmType = "Microsoft.Insights/diagnosticSettings@2021-05-01-preview";
    private const string ZoneRedundantPropertyName = "zoneRedundant";
    private const string VnetConfigurationPropertyName = "vnetConfiguration";
    private const string InternalPropertyName = "internal";
    private const string AppLogsConfigurationPropertyName = "appLogsConfiguration";
    private const string DestinationPropertyName = "destination";
    private const string AzureMonitorDestinationValue = "azure-monitor";
    private const string WorkloadProfilesPropertyName = "workloadProfiles";
    private const string WorkspaceIdPropertyName = "workspaceId";
    private const string LogsPropertyName = "logs";
    private const string CategoryGroupPropertyName = "categoryGroup";
    private const string AllLogsCategoryGroupValue = "allLogs";
    private const string EnabledPropertyName = "enabled";
    private const string DiagnosticSettingsNameValue = "containerAppEnvLogs";
    private const string LogAnalyticsWorkspaceProvidedExpression = LogAnalyticsWorkspaceIdParameterName + " != ''";
    private const string NullExpression = "null";
    private const string EmptyParameterValue = "";
    private const string DefaultWorkloadProfileType = "Consumption";
    private const string DefaultDomainOutputName = "defaultDomain";
    private const string StaticIpOutputName = "staticIp";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string DefaultDomainExpression = ResourceSymbol + ".properties.defaultDomain";
    private const string StaticIpExpression = ResourceSymbol + ".properties.staticIp";
    private const string WorkloadProfileTypeUnion = "'Consumption' | 'D4' | 'D8' | 'D16' | 'D32' | 'E4' | 'E8' | 'E16' | 'E32'";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerAppEnvironment;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ContainerAppEnvironment;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, WorkloadProfileTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Container App Environment")
            .Param(NameParameterName, BicepType.String, "Name of the Container App Environment")
            .Param(WorkloadProfileTypeParameterName, BicepType.Custom(WorkloadProfileTypeName), "Workload profile type",
                defaultValue: new BicepStringLiteral(DefaultWorkloadProfileType))
            .Param(InternalLoadBalancerEnabledParameterName, BicepType.Bool, "Whether the internal load balancer is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(ZoneRedundancyEnabledParameterName, BicepType.Bool, "Whether zone redundancy is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(LogAnalyticsWorkspaceIdParameterName, BicepType.String,
                "Resource ID of the Log Analytics workspace. When provided, logs are routed to this workspace via Azure Monitor — no shared key required.",
                defaultValue: new BicepStringLiteral(EmptyParameterValue))
            .Resource(ResourceSymbol, ContainerAppEnvironmentArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(ZoneRedundantPropertyName, new BicepReference(ZoneRedundancyEnabledParameterName))
                .Property(VnetConfigurationPropertyName, vnet => vnet
                    .Property(InternalPropertyName, new BicepReference(InternalLoadBalancerEnabledParameterName)))
                .Property(AppLogsConfigurationPropertyName, new BicepConditionalExpression(
                    new BicepRawExpression(LogAnalyticsWorkspaceProvidedExpression),
                    new BicepObjectExpression([
                        new BicepPropertyAssignment(DestinationPropertyName, new BicepStringLiteral(AzureMonitorDestinationValue)),
                    ]),
                    new BicepRawExpression(NullExpression)))
                .Property(WorkloadProfilesPropertyName, new BicepArrayExpression([
                    new BicepObjectExpression([
                        new BicepPropertyAssignment(NamePropertyName, new BicepReference(WorkloadProfileTypeParameterName)),
                        new BicepPropertyAssignment(WorkloadProfileTypeParameterName, new BicepReference(WorkloadProfileTypeParameterName)),
                    ])
                ])))
            .AdditionalResource(DiagnosticSettingsResourceName, DiagnosticSettingsArmType,
                condition: new BicepRawExpression(LogAnalyticsWorkspaceProvidedExpression),
                scope: ResourceSymbol,
                bodyBuilder: body => body
                    .Property(NamePropertyName, new BicepStringLiteral(DiagnosticSettingsNameValue))
                    .Property(PropertiesPropertyName, p => p
                        .Property(WorkspaceIdPropertyName, new BicepReference(LogAnalyticsWorkspaceIdParameterName))
                        .Property(LogsPropertyName, new BicepArrayExpression([
                            new BicepObjectExpression([
                                new BicepPropertyAssignment(CategoryGroupPropertyName, new BicepStringLiteral(AllLogsCategoryGroupValue)),
                                new BicepPropertyAssignment(EnabledPropertyName, new BicepBoolLiteral(true)),
                            ])
                        ]))))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Container App Environment")
            .Output(DefaultDomainOutputName, BicepType.String,
                new BicepRawExpression(DefaultDomainExpression),
                description: "The default domain of the Container App Environment")
            .Output(StaticIpOutputName, BicepType.String,
                new BicepRawExpression(StaticIpExpression),
                description: "The static IP of the Container App Environment")
            .ExportedType(WorkloadProfileTypeName,
                new BicepRawExpression(WorkloadProfileTypeUnion),
                description: "Workload profile type for the Container App Environment")
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
            ModuleBicepContent = ContainerAppEnvironmentModuleTemplate,
            ModuleTypesBicepContent = ContainerAppEnvironmentTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ContainerAppEnvironmentTypesTemplate = """
        @export()
        @description('Workload profile type for the Container App Environment')
        type WorkloadProfileType = 'Consumption' | 'D4' | 'D8' | 'D16' | 'D32' | 'E4' | 'E8' | 'E16' | 'E32'
        """;

    private const string ContainerAppEnvironmentModuleTemplate = """
        import { WorkloadProfileType } from './types.bicep'

        @description('Azure region for the Container App Environment')
        param location string

        @description('Name of the Container App Environment')
        param name string

        @description('Workload profile type')
        param workloadProfileType WorkloadProfileType = 'Consumption'

        @description('Whether the internal load balancer is enabled')
        param internalLoadBalancerEnabled bool = false

        @description('Whether zone redundancy is enabled')
        param zoneRedundancyEnabled bool = false

        @description('Resource ID of the Log Analytics workspace. When provided, logs are routed to this workspace via Azure Monitor — no shared key required.')
        param logAnalyticsWorkspaceId string = ''

        resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
          name: name
          location: location
          properties: {
            zoneRedundant: zoneRedundancyEnabled
            vnetConfiguration: {
              internal: internalLoadBalancerEnabled
            }
            appLogsConfiguration: logAnalyticsWorkspaceId != '' ? {
              destination: 'azure-monitor'
            } : null
            workloadProfiles: [
              {
                name: workloadProfileType
                workloadProfileType: workloadProfileType
              }
            ]
          }
        }

        resource diagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (logAnalyticsWorkspaceId != '') {
          name: 'containerAppEnvLogs'
          scope: containerAppEnv
          properties: {
            workspaceId: logAnalyticsWorkspaceId
            logs: [
              {
                categoryGroup: 'allLogs'
                enabled: true
              }
            ]
          }
        }

        @description('The resource ID of the Container App Environment')
        output id string = containerAppEnv.id

        @description('The default domain of the Container App Environment')
        output defaultDomain string = containerAppEnv.properties.defaultDomain

        @description('The static IP of the Container App Environment')
        output staticIp string = containerAppEnv.properties.staticIp
        """;
}
