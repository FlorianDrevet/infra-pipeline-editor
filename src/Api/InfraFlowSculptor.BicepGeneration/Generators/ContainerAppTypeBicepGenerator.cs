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
            ModuleFileName = "containerApp",
            ModuleFolderName = "ContainerApp",
            ModuleBicepContent = ContainerAppModuleTemplate,
            ModuleTypesBicepContent = ContainerAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ContainerAppTypesTemplate = """
        @export()
        @description('Ingress transport method for the Container App')
        type TransportMethod = 'auto' | 'http' | 'http2' | 'tcp'
        """;

    private const string ContainerAppModuleTemplate = """
        import { TransportMethod } from './types.bicep'

        @description('Azure region for the Container App')
        param location string

        @description('Name of the Container App')
        param name string

        @description('Resource ID of the Container App Environment')
        param containerAppEnvironmentId string

        @description('Container image to deploy')
        param containerImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

        @description('CPU cores allocated to the container')
        param cpuCores string = '0.25'

        @description('Memory allocated to the container (e.g. 0.5Gi)')
        param memoryGi string = '0.5Gi'

        @description('Minimum number of replicas')
        param minReplicas int = 0

        @description('Maximum number of replicas')
        param maxReplicas int = 1

        @description('Whether ingress is enabled')
        param ingressEnabled bool = true

        @description('Target port for ingress traffic')
        param ingressTargetPort int = 80

        @description('Whether ingress is externally accessible')
        param ingressExternal bool = true

        @description('Transport method for ingress')
        param transportMethod TransportMethod = 'auto'

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
