namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed partial class ContainerAppTypeBicepGenerator
{
  private const string CustomDomainDeclarationsPlaceholder = "__CUSTOM_DOMAIN_DECLARATIONS__";
  private const string IngressCustomDomainsPropertyPlaceholder = "__INGRESS_CUSTOM_DOMAINS_PROPERTY__";

  private const string CustomDomainDeclarationsBlock = """
    @description('Custom domain bindings for this Container App')
    param customDomains array = []

    var customDomainBindings = [for domain in customDomains: {
      name: domain.domainName
      bindingType: domain.bindingType
    }]
    """;

  private const string IngressCustomDomainsPropertyBlock = """
        customDomains: !empty(customDomains) ? customDomainBindings : null
    """;

    private const string ContainerAppTypesTemplate = """
        @export()
        @description('Ingress transport method for the Container App')
        type TransportMethod = 'auto' | 'http' | 'http2' | 'tcp'

        @export()
        @description('Container runtime configuration (image, CPU, memory)')
        type ContainerRuntimeConfig = {
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

    private static readonly string ContainerAppModuleTemplate = $$"""
        import { ContainerRuntimeConfig, ScalingConfig, IngressConfig, HealthProbeConfig } from './types.bicep'

        @description('Azure region for the Container App')
        param location string

        @description('Name of the Container App')
        param name string

        @description('Resource ID of the Container App Environment')
        param containerAppEnvironmentId string

        @description('Container image (overridden by app pipeline after first deploy)')
        param containerImage string = '{{DefaultContainerImage}}'

        @description('Container runtime configuration')
        param containerRuntime ContainerRuntimeConfig

        @description('Scaling configuration')
        param scaling ScalingConfig

        @description('Ingress configuration')
        param ingress IngressConfig

        @description('Health probe configuration')
        param healthProbes HealthProbeConfig

        {{CustomDomainDeclarationsPlaceholder}}

        resource containerApp '{{ContainerAppArmType}}' = {
          name: name
          location: location
          properties: {
            managedEnvironmentId: containerAppEnvironmentId
            configuration: {
              ingress: ingress.enabled ? {
                external: ingress.external
                targetPort: ingress.targetPort
                transport: ingress.transportMethod
        {{IngressCustomDomainsPropertyPlaceholder}}
              } : null
            }
            template: {
              containers: [
                {
                  name: name
                  image: containerImage
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

    private static readonly string ContainerAppWithAcrManagedIdentityModuleTemplate = $$"""
        import { ContainerRuntimeConfig, ScalingConfig, IngressConfig, HealthProbeConfig } from './types.bicep'

        @description('Azure region for the Container App')
        param location string

        @description('Name of the Container App')
        param name string

        @description('Resource ID of the Container App Environment')
        param containerAppEnvironmentId string

        @description('Container image (overridden by app pipeline after first deploy)')
        param containerImage string = '{{DefaultContainerImage}}'

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

        {{CustomDomainDeclarationsPlaceholder}}

        resource containerApp '{{ContainerAppArmType}}' = {
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
        {{IngressCustomDomainsPropertyPlaceholder}}
              } : null
            }
            template: {
              containers: [
                {
                  name: name
                  image: containerImage
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

    private static readonly string ContainerAppWithAcrAdminCredentialsModuleTemplate = $$"""
        import { ContainerRuntimeConfig, ScalingConfig, IngressConfig, HealthProbeConfig } from './types.bicep'

        @description('Azure region for the Container App')
        param location string

        @description('Name of the Container App')
        param name string

        @description('Resource ID of the Container App Environment')
        param containerAppEnvironmentId string

        @description('Container image (overridden by app pipeline after first deploy)')
        param containerImage string = '{{DefaultContainerImage}}'

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

        {{CustomDomainDeclarationsPlaceholder}}
        var acrUsername = split(acrLoginServer, '.')[0]
        var acrPasswordSecretName = 'acr-password'

        resource containerApp '{{ContainerAppArmType}}' = {
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
        {{IngressCustomDomainsPropertyPlaceholder}}
              } : null
            }
            template: {
              containers: [
                {
                  name: name
                  image: containerImage
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