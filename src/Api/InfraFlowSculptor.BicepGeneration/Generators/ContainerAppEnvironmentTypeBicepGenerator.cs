using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App Environment (<c>Microsoft.App/managedEnvironments</c>).
/// </summary>
public sealed class ContainerAppEnvironmentTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.App/managedEnvironments";

    /// <inheritdoc />
    public string ResourceTypeName => "ContainerAppEnvironment";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "containerAppEnvironment",
            ModuleFileName = "containerAppEnvironment.bicep",
            ModuleFolderName = "ContainerAppEnvironment",
            ModuleBicepContent = ContainerAppEnvironmentModuleTemplate,
            ModuleTypesBicepContent = ContainerAppEnvironmentTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ContainerAppEnvironmentTypesTemplate = """
        @export()
        @description('SKU for the Container App Environment')
        type SkuName = 'Consumption' | 'Premium'

        @export()
        @description('Workload profile type for the Container App Environment')
        type WorkloadProfileType = 'Consumption' | 'D4' | 'D8' | 'D16' | 'D32' | 'E4' | 'E8' | 'E16' | 'E32'
        """;

    private const string ContainerAppEnvironmentModuleTemplate = """
        import { SkuName, WorkloadProfileType } from './types.bicep'

        @description('Azure region for the Container App Environment')
        param location string

        @description('Name of the Container App Environment')
        param name string

        @description('SKU of the Container App Environment')
        param sku SkuName = 'Consumption'

        @description('Workload profile type')
        param workloadProfileType WorkloadProfileType = 'Consumption'

        @description('Whether the internal load balancer is enabled')
        param internalLoadBalancerEnabled bool = false

        @description('Whether zone redundancy is enabled')
        param zoneRedundancyEnabled bool = false

        @description('Resource ID of the Log Analytics workspace (empty to skip)')
        param logAnalyticsWorkspaceId string = ''

        resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
          name: name
          location: location
          properties: {
            zoneRedundant: zoneRedundancyEnabled
            vnetConfiguration: {
              internal: internalLoadBalancerEnabled
            }
            appLogsConfiguration: {
              destination: 'log-analytics'
              logAnalyticsConfiguration: logAnalyticsWorkspaceId != '' ? {
                customerId: logAnalyticsWorkspaceId
              } : null
            }
            workloadProfiles: [
              {
                name: workloadProfileType
                workloadProfileType: workloadProfileType
              }
            ]
          }
        }
        """;
}
