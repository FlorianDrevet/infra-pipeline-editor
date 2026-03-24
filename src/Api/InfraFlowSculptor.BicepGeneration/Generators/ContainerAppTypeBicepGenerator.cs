using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App (<c>Microsoft.App/containerApps</c>).
/// </summary>
public sealed class ContainerAppTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.App/containerApps";

    /// <inheritdoc />
    public string ResourceTypeName => "ContainerApp";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "containerApp",
            ModuleFileName = "containerApp.bicep",
            ModuleBicepContent = ContainerAppModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ContainerAppModuleTemplate = """
        param location string
        param name string
        param containerAppEnvironmentId string
        param containerImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
        param cpuCores string = '0.25'
        param memoryGi string = '0.5Gi'
        param minReplicas int = 0
        param maxReplicas int = 1
        param ingressEnabled bool = true
        param ingressTargetPort int = 80
        param ingressExternal bool = true
        param transportMethod string = 'auto'

        resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
          name: name
          location: location
          properties: {
            managedEnvironmentId: containerAppEnvironmentId
            configuration: {
              ingress: ingressEnabled ? {
                external: ingressExternal
                targetPort: ingressTargetPort
                transport: transportMethod
              } : null
            }
            template: {
              containers: [
                {
                  name: name
                  image: containerImage
                  resources: {
                    cpu: json(cpuCores)
                    memory: memoryGi
                  }
                }
              ]
              scale: {
                minReplicas: minReplicas
                maxReplicas: maxReplicas
              }
            }
          }
        }
        """;
}
