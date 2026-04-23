using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App (<c>Microsoft.App/containerApps</c>).
/// Supports optional ACR integration with managed identity or admin credentials-based image pulling.
/// </summary>
public sealed class ContainerAppTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
  private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
  private const string AdminCredentialsAcrAuthMode = "AdminCredentials";

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
      var acrAuthMode = GetAcrAuthMode(resource.Properties);
      var useAdminCredentials = hasAcr
        && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);
        var hasCustomDomains = resource.CustomDomains.Count > 0;

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
                readiness = new { path = "", port = 0 },
                liveness = new { path = "", port = 0 },
                startup = new { path = "", port = 0 }
            }
        };

        if (hasAcr)
        {
            parameters["acrLoginServer"] = "";
          if (!useAdminCredentials)
          {
            parameters["acrManagedIdentityClientId"] = "";
          }
        }

        if (hasCustomDomains)
        {
            parameters["customDomains"] = new List<object>();
        }

        return new GeneratedTypeModule
        {
            ModuleName = "containerApp",
            ModuleFileName = "containerApp",
            ModuleFolderName = "ContainerApp",
          ModuleBicepContent = hasAcr
            ? useAdminCredentials
              ? ContainerAppWithAcrAdminCredentialsModuleTemplate
              : ContainerAppWithAcrManagedIdentityModuleTemplate
            : ContainerAppModuleTemplate,
            ModuleTypesBicepContent = ContainerAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = parameters,
          SecureParameters = useAdminCredentials ? ["acrPassword"] : [],
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
                ["readinessProbePath"] = ("healthProbes", "readiness.path"),
                ["readinessProbePort"] = ("healthProbes", "readiness.port"),
                ["livenessProbePath"] = ("healthProbes", "liveness.path"),
                ["livenessProbePort"] = ("healthProbes", "liveness.port"),
                ["startupProbePath"] = ("healthProbes", "startup.path"),
                ["startupProbePort"] = ("healthProbes", "startup.port")
            }
        };
    }

          private static string GetAcrAuthMode(IReadOnlyDictionary<string, string> properties)
          {
            var acrAuthMode = properties.GetValueOrDefault("acrAuthMode", string.Empty);
            return string.IsNullOrWhiteSpace(acrAuthMode)
              ? ManagedIdentityAcrAuthMode
              : acrAuthMode;
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
        @description('Configuration for a single HTTP health probe')
        type ProbeConfig = {
          @description('HTTP path for the probe (empty to disable)')
          path: string
          @description('Port for the probe (0 to disable)')
          port: int
        }

        @export()
        @description('Health probe configuration for the Container App')
        type HealthProbeConfig = {
          @description('Readiness probe configuration')
          readiness: ProbeConfig
          @description('Liveness probe configuration')
          liveness: ProbeConfig
          @description('Startup probe configuration')
          startup: ProbeConfig
        }
        """;

    private const string ContainerAppModuleTemplate = """
        import { ContainerRuntimeConfig, ScalingConfig, IngressConfig, HealthProbeConfig } from './types.bicep'

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

        @description('Custom domain bindings for this Container App')
        param customDomains array = []

        var customDomainBindings = [for domain in customDomains: {
          name: domain.domainName
          bindingType: domain.bindingType
        }]

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
                customDomains: !empty(customDomains) ? customDomainBindings : null
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
                    !empty(healthProbes.readiness.path) && healthProbes.readiness.port > 0 ? [{
                      type: 'Readiness'
                      httpGet: {
                        path: healthProbes.readiness.path
                        port: healthProbes.readiness.port
                      }
                    }] : [],
                    !empty(healthProbes.liveness.path) && healthProbes.liveness.port > 0 ? [{
                      type: 'Liveness'
                      httpGet: {
                        path: healthProbes.liveness.path
                        port: healthProbes.liveness.port
                      }
                    }] : [],
                    !empty(healthProbes.startup.path) && healthProbes.startup.port > 0 ? [{
                      type: 'Startup'
                      httpGet: {
                        path: healthProbes.startup.path
                        port: healthProbes.startup.port
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

    private const string ContainerAppWithAcrManagedIdentityModuleTemplate = """
        import { ContainerRuntimeConfig, ScalingConfig, IngressConfig, HealthProbeConfig } from './types.bicep'

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

        @description('Custom domain bindings for this Container App')
        param customDomains array = []

        var customDomainBindings = [for domain in customDomains: {
          name: domain.domainName
          bindingType: domain.bindingType
        }]

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
                customDomains: !empty(customDomains) ? customDomainBindings : null
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
                    !empty(healthProbes.readiness.path) && healthProbes.readiness.port > 0 ? [{
                      type: 'Readiness'
                      httpGet: {
                        path: healthProbes.readiness.path
                        port: healthProbes.readiness.port
                      }
                    }] : [],
                    !empty(healthProbes.liveness.path) && healthProbes.liveness.port > 0 ? [{
                      type: 'Liveness'
                      httpGet: {
                        path: healthProbes.liveness.path
                        port: healthProbes.liveness.port
                      }
                    }] : [],
                    !empty(healthProbes.startup.path) && healthProbes.startup.port > 0 ? [{
                      type: 'Startup'
                      httpGet: {
                        path: healthProbes.startup.path
                        port: healthProbes.startup.port
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

    private const string ContainerAppWithAcrAdminCredentialsModuleTemplate = """
        import { ContainerRuntimeConfig, ScalingConfig, IngressConfig, HealthProbeConfig } from './types.bicep'

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

        @secure()
        @description('Admin password for the Container Registry')
        param acrPassword string

        @description('Custom domain bindings for this Container App')
        param customDomains array = []

        var customDomainBindings = [for domain in customDomains: {
          name: domain.domainName
          bindingType: domain.bindingType
        }]
        var acrUsername = split(acrLoginServer, '.')[0]
        var acrPasswordSecretName = 'acr-password'

        resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
          name: name
          location: location
          properties: {
            managedEnvironmentId: containerAppEnvironmentId
            configuration: {
              secrets: [
                {
                  name: acrPasswordSecretName
                  value: acrPassword
                }
              ]
              registries: [
                {
                  server: acrLoginServer
                  username: acrUsername
                  passwordSecretRef: acrPasswordSecretName
                }
              ]
              ingress: ingress.enabled ? {
                external: ingress.external
                targetPort: ingress.targetPort
                transport: ingress.transportMethod
                customDomains: !empty(customDomains) ? customDomainBindings : null
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
                    !empty(healthProbes.readiness.path) && healthProbes.readiness.port > 0 ? [{
                      type: 'Readiness'
                      httpGet: {
                        path: healthProbes.readiness.path
                        port: healthProbes.readiness.port
                      }
                    }] : [],
                    !empty(healthProbes.liveness.path) && healthProbes.liveness.port > 0 ? [{
                      type: 'Liveness'
                      httpGet: {
                        path: healthProbes.liveness.path
                        port: healthProbes.liveness.port
                      }
                    }] : [],
                    !empty(healthProbes.startup.path) && healthProbes.startup.port > 0 ? [{
                      type: 'Startup'
                      httpGet: {
                        path: healthProbes.startup.path
                        port: healthProbes.startup.port
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
