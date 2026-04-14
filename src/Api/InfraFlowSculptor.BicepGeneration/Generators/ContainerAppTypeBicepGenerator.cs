using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App (<c>Microsoft.App/containerApps</c>).
/// Supports optional ACR integration with managed identity-based image pulling.
/// </summary>
public sealed class ContainerAppTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerApp;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ContainerApp;

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var containerRegistryId = resource.Properties.GetValueOrDefault("containerRegistryId", "");
        var hasAcr = !string.IsNullOrEmpty(containerRegistryId);

        var dockerImageName = resource.Properties.GetValueOrDefault("dockerImageName", "");
        var containerImage = !string.IsNullOrEmpty(dockerImageName)
            ? dockerImageName
            : "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest";

        var parameters = new Dictionary<string, object>
        {
            ["containerImage"] = containerImage,
            ["cpuCores"] = "0.25",
            ["memoryGi"] = "0.5Gi",
            ["minReplicas"] = 0,
            ["maxReplicas"] = 1,
            ["ingressEnabled"] = true,
            ["ingressTargetPort"] = 80,
            ["ingressExternal"] = true,
            ["transportMethod"] = "auto"
        };

        if (hasAcr)
        {
            parameters["acrLoginServer"] = "";
            parameters["acrManagedIdentityClientId"] = "";
        }

        return new GeneratedTypeModule
        {
            ModuleName = "containerApp",
            ModuleFileName = "containerApp",
            ModuleFolderName = "ContainerApp",
            ModuleBicepContent = hasAcr ? ContainerAppWithAcrModuleTemplate : ContainerAppModuleTemplate,
            ModuleTypesBicepContent = ContainerAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = parameters
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

        @description('The resource ID of the Container App')
        output id string = containerApp.id

        @description('The FQDN of the Container App')
        output fqdn string = containerApp.properties.configuration.ingress != null ? containerApp.properties.configuration.ingress.fqdn : ''

        @description('The latest revision FQDN of the Container App')
        output latestRevisionFqdn string = containerApp.properties.latestRevisionFqdn
        """;

    private const string ContainerAppWithAcrModuleTemplate = """
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

        @description('ACR login server (e.g. myregistry.azurecr.io)')
        param acrLoginServer string

        @description('Client ID of the managed identity for ACR pull')
        param acrManagedIdentityClientId string = ''

        resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
          name: name
          location: location
          properties: {
            managedEnvironmentId: containerAppEnvironmentId
            configuration: {
              registries: [
                {
                  server: acrLoginServer
                  identity: !empty(acrManagedIdentityClientId) ? acrManagedIdentityClientId : 'system'
                }
              ]
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

        @description('The resource ID of the Container App')
        output id string = containerApp.id

        @description('The FQDN of the Container App')
        output fqdn string = containerApp.properties.configuration.ingress != null ? containerApp.properties.configuration.ingress.fqdn : ''

        @description('The latest revision FQDN of the Container App')
        output latestRevisionFqdn string = containerApp.properties.latestRevisionFqdn
        """;
}
