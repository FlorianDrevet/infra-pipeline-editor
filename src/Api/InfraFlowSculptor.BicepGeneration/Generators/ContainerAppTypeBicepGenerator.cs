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
            ["containerRuntime"] = new { image = containerImage, cpuCores = "0.25", memoryGi = "0.5Gi" },
            ["scaling"] = new { minReplicas = 0, maxReplicas = 1 },
            ["ingress"] = new { enabled = true, targetPort = 80, external = true, transportMethod = "auto" },
            ["healthProbes"] = new
            {
                readinessPath = "", readinessPort = 0,
                livenessPath = "", livenessPort = 0,
                startupPath = "", startupPort = 0
            }
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
            Parameters = parameters,
            ParameterTypeOverrides = new Dictionary<string, string>
            {
                ["containerRuntime"] = "ContainerRuntimeConfig",
                ["scaling"] = "ScalingConfig",
                ["ingress"] = "IngressConfig",
                ["healthProbes"] = "HealthProbeConfig"
            },
            ParameterGroupMappings = new Dictionary<string, (string, string)>
            {
                ["cpuCores"] = ("containerRuntime", "cpuCores"),
                ["memoryGi"] = ("containerRuntime", "memoryGi"),
                ["minReplicas"] = ("scaling", "minReplicas"),
                ["maxReplicas"] = ("scaling", "maxReplicas"),
                ["ingressEnabled"] = ("ingress", "enabled"),
                ["ingressTargetPort"] = ("ingress", "targetPort"),
                ["ingressExternal"] = ("ingress", "external"),
                ["transportMethod"] = ("ingress", "transportMethod"),
                ["readinessProbePath"] = ("healthProbes", "readinessPath"),
                ["readinessProbePort"] = ("healthProbes", "readinessPort"),
                ["livenessProbePath"] = ("healthProbes", "livenessPath"),
                ["livenessProbePort"] = ("healthProbes", "livenessPort"),
                ["startupProbePath"] = ("healthProbes", "startupPath"),
                ["startupProbePort"] = ("healthProbes", "startupPort")
            }
        };
    }

    private const string ContainerAppTypesTemplate = """
        @export()
        @description('Ingress transport method for the Container App')
        type TransportMethod = 'auto' | 'http' | 'http2' | 'tcp'

        @export()
        @description('Container runtime configuration (image, CPU, memory)')
        type ContainerRuntimeConfig = {
          @description('Container image to deploy')
          image: string
          @description('CPU cores allocated to the container')
          cpuCores: string
          @description('Memory allocated to the container (e.g. 0.5Gi)')
          memoryGi: string
        }

        @export()
        @description('Scaling configuration for the Container App')
        type ScalingConfig = {
          @description('Minimum number of replicas')
          minReplicas: int
          @description('Maximum number of replicas')
          maxReplicas: int
        }

        @export()
        @description('Ingress configuration for the Container App')
        type IngressConfig = {
          @description('Whether ingress is enabled')
          enabled: bool
          @description('Target port for ingress traffic')
          targetPort: int
          @description('Whether ingress is externally accessible')
          external: bool
          @description('Transport method for ingress')
          transportMethod: TransportMethod
        }

        @export()
        @description('Health probe configuration for the Container App')
        type HealthProbeConfig = {
          @description('HTTP path for the readiness probe (empty to disable)')
          readinessPath: string
          @description('Port for the readiness probe (0 to disable)')
          readinessPort: int
          @description('HTTP path for the liveness probe (empty to disable)')
          livenessPath: string
          @description('Port for the liveness probe (0 to disable)')
          livenessPort: int
          @description('HTTP path for the startup probe (empty to disable)')
          startupPath: string
          @description('Port for the startup probe (0 to disable)')
          startupPort: int
        }
        """;

    private const string ContainerAppModuleTemplate = """
        import { TransportMethod, ContainerRuntimeConfig, ScalingConfig, IngressConfig, HealthProbeConfig } from './types.bicep'

        @description('Azure region for the Container App')
        param location string

        @description('Name of the Container App')
        param name string

        @description('Resource ID of the Container App Environment')
        param containerAppEnvironmentId string

        @description('Container runtime configuration')
        param containerRuntime ContainerRuntimeConfig

        @description('Scaling configuration')
        param scaling ScalingConfig

        @description('Ingress configuration')
        param ingress IngressConfig

        @description('Health probe configuration')
        param healthProbes HealthProbeConfig

        resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
          name: name
          location: location
          properties: {
            managedEnvironmentId: containerAppEnvironmentId
            configuration: {
              ingress: ingress.enabled ? {
                external: ingress.external
                targetPort: ingress.targetPort
                transport: ingress.transportMethod
              } : null
            }
            template: {
              containers: [
                {
                  name: name
                  image: containerRuntime.image
                  resources: {
                    cpu: json(containerRuntime.cpuCores)
                    memory: containerRuntime.memoryGi
                  }
                  probes: union(
                    !empty(healthProbes.readinessPath) && healthProbes.readinessPort > 0 ? [{
                      type: 'Readiness'
                      httpGet: {
                        path: healthProbes.readinessPath
                        port: healthProbes.readinessPort
                      }
                    }] : [],
                    !empty(healthProbes.livenessPath) && healthProbes.livenessPort > 0 ? [{
                      type: 'Liveness'
                      httpGet: {
                        path: healthProbes.livenessPath
                        port: healthProbes.livenessPort
                      }
                    }] : [],
                    !empty(healthProbes.startupPath) && healthProbes.startupPort > 0 ? [{
                      type: 'Startup'
                      httpGet: {
                        path: healthProbes.startupPath
                        port: healthProbes.startupPort
                      }
                    }] : []
                  )
                }
              ]
              scale: {
                minReplicas: scaling.minReplicas
                maxReplicas: scaling.maxReplicas
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
        import { TransportMethod, ContainerRuntimeConfig, ScalingConfig, IngressConfig, HealthProbeConfig } from './types.bicep'

        @description('Azure region for the Container App')
        param location string

        @description('Name of the Container App')
        param name string

        @description('Resource ID of the Container App Environment')
        param containerAppEnvironmentId string

        @description('Container runtime configuration')
        param containerRuntime ContainerRuntimeConfig

        @description('Scaling configuration')
        param scaling ScalingConfig

        @description('Ingress configuration')
        param ingress IngressConfig

        @description('Health probe configuration')
        param healthProbes HealthProbeConfig

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
              ingress: ingress.enabled ? {
                external: ingress.external
                targetPort: ingress.targetPort
                transport: ingress.transportMethod
              } : null
            }
            template: {
              containers: [
                {
                  name: name
                  image: containerRuntime.image
                  resources: {
                    cpu: json(containerRuntime.cpuCores)
                    memory: containerRuntime.memoryGi
                  }
                  probes: union(
                    !empty(healthProbes.readinessPath) && healthProbes.readinessPort > 0 ? [{
                      type: 'Readiness'
                      httpGet: {
                        path: healthProbes.readinessPath
                        port: healthProbes.readinessPort
                      }
                    }] : [],
                    !empty(healthProbes.livenessPath) && healthProbes.livenessPort > 0 ? [{
                      type: 'Liveness'
                      httpGet: {
                        path: healthProbes.livenessPath
                        port: healthProbes.livenessPort
                      }
                    }] : [],
                    !empty(healthProbes.startupPath) && healthProbes.startupPort > 0 ? [{
                      type: 'Startup'
                      httpGet: {
                        path: healthProbes.startupPath
                        port: healthProbes.startupPort
                      }
                    }] : []
                  )
                }
              ]
              scale: {
                minReplicas: scaling.minReplicas
                maxReplicas: scaling.maxReplicas
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
