using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App Environment (<c>Microsoft.App/managedEnvironments</c>).
/// </summary>
public sealed class ContainerAppEnvironmentTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerAppEnvironment;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ContainerAppEnvironment;

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
