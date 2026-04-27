using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App Environment (<c>Microsoft.App/managedEnvironments@2024-03-01</c>).
/// </summary>
public sealed class ContainerAppEnvironmentTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerAppEnvironment;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ContainerAppEnvironment;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("containerAppEnvironment", "ContainerAppEnvironment", ResourceTypeName)
            .Import("./types.bicep", "WorkloadProfileType")
            .Param("location", BicepType.String, "Azure region for the Container App Environment")
            .Param("name", BicepType.String, "Name of the Container App Environment")
            .Param("workloadProfileType", BicepType.Custom("WorkloadProfileType"), "Workload profile type",
                defaultValue: new BicepStringLiteral("Consumption"))
            .Param("internalLoadBalancerEnabled", BicepType.Bool, "Whether the internal load balancer is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("zoneRedundancyEnabled", BicepType.Bool, "Whether zone redundancy is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("logAnalyticsWorkspaceId", BicepType.String,
                "Resource ID of the Log Analytics workspace. When provided, logs are routed to this workspace via Azure Monitor — no shared key required.",
                defaultValue: new BicepStringLiteral(""))
            .Resource("containerAppEnv", "Microsoft.App/managedEnvironments@2024-03-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("properties", props => props
                .Property("zoneRedundant", new BicepReference("zoneRedundancyEnabled"))
                .Property("vnetConfiguration", vnet => vnet
                    .Property("internal", new BicepReference("internalLoadBalancerEnabled")))
                .Property("appLogsConfiguration", new BicepConditionalExpression(
                    new BicepRawExpression("logAnalyticsWorkspaceId != ''"),
                    new BicepObjectExpression([
                        new BicepPropertyAssignment("destination", new BicepStringLiteral("azure-monitor")),
                    ]),
                    new BicepRawExpression("null")))
                .Property("workloadProfiles", new BicepArrayExpression([
                    new BicepObjectExpression([
                        new BicepPropertyAssignment("name", new BicepReference("workloadProfileType")),
                        new BicepPropertyAssignment("workloadProfileType", new BicepReference("workloadProfileType")),
                    ])
                ])))
            .AdditionalResource("diagnosticSettings", "Microsoft.Insights/diagnosticSettings@2021-05-01-preview",
                condition: new BicepRawExpression("logAnalyticsWorkspaceId != ''"),
                scope: "containerAppEnv",
                bodyBuilder: body => body
                    .Property("name", new BicepStringLiteral("containerAppEnvLogs"))
                    .Property("properties", p => p
                        .Property("workspaceId", new BicepReference("logAnalyticsWorkspaceId"))
                        .Property("logs", new BicepArrayExpression([
                            new BicepObjectExpression([
                                new BicepPropertyAssignment("categoryGroup", new BicepStringLiteral("allLogs")),
                                new BicepPropertyAssignment("enabled", new BicepBoolLiteral(true)),
                            ])
                        ]))))
            .Output("id", BicepType.String, new BicepRawExpression("containerAppEnv.id"),
                description: "The resource ID of the Container App Environment")
            .Output("defaultDomain", BicepType.String,
                new BicepRawExpression("containerAppEnv.properties.defaultDomain"),
                description: "The default domain of the Container App Environment")
            .Output("staticIp", BicepType.String,
                new BicepRawExpression("containerAppEnv.properties.staticIp"),
                description: "The static IP of the Container App Environment")
            .ExportedType("WorkloadProfileType",
                new BicepRawExpression("'Consumption' | 'D4' | 'D8' | 'D16' | 'D32' | 'E4' | 'E8' | 'E16' | 'E32'"),
                description: "Workload profile type for the Container App Environment")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "containerAppEnvironment",
            ModuleFileName = "containerAppEnvironment",
            ModuleFolderName = "ContainerAppEnvironment",
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
