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
            ModuleBicepContent = ContainerAppEnvironmentModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ContainerAppEnvironmentModuleTemplate = """
        param location string
        param name string
        param sku string = 'Consumption'
        param workloadProfileType string = 'Consumption'
        param internalLoadBalancerEnabled bool = false
        param zoneRedundancyEnabled bool = false
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
