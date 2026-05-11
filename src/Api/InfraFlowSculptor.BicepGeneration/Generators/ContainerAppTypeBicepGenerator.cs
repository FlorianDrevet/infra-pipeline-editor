using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels.ContainerApp;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App (<c>Microsoft.App/containerApps</c>).
/// Supports optional ACR integration with managed identity or admin credentials-based image pulling.
/// </summary>
public sealed class ContainerAppTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
    private const string AdminCredentialsAcrAuthMode = "AdminCredentials";

    private const string ModuleName = "containerApp";
    private const string ModuleFolderName = "ContainerApp";
    private const string ManagedIdentityModuleFileName = "containerAppAcrManagedIdentity";
    private const string AdminCredentialsModuleFileName = "containerAppAcrAdminCredentials";
    private const string ResourceSymbol = "containerApp";

    private const string ContainerRuntimeParameterName = "containerRuntime";
    private const string ScalingParameterName = "scaling";
    private const string IngressParameterName = "ingress";
    private const string HealthProbesParameterName = "healthProbes";

    private const string ContainerRuntimeConfigTypeName = "ContainerRuntimeConfig";
    private const string ScalingConfigTypeName = "ScalingConfig";
    private const string IngressConfigTypeName = "IngressConfig";
    private const string HealthProbeConfigTypeName = "HealthProbeConfig";
    private const string TransportMethodTypeName = "TransportMethod";
    private const string ProbeConfigTypeName = "ProbeConfig";
    private const string AcrAuthModePropertyName = "acrAuthMode";
    private const string ContainerRegistryIdPropertyName = "containerRegistryId";
    private const string DockerImageNamePropertyName = "dockerImageName";
    private const string ContainerAppEnvironmentIdParameterName = "containerAppEnvironmentId";
    private const string AcrLoginServerParameterName = "acrLoginServer";
    private const string AcrPasswordParameterName = "acrPassword";
    private const string AcrManagedIdentityClientIdParameterName = "acrManagedIdentityClientId";
    private const string CustomDomainsParameterName = "customDomains";
    private const string CustomDomainBindingsVariableName = "customDomainBindings";
    private const string AcrUsernameVariableName = "acrUsername";
    private const string AcrPasswordSecretNameVariableName = "acrPasswordSecretName";
    private const string ContainerAppArmType = "Microsoft.App/containerApps@2024-03-01";
    private const string DefaultContainerImage = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest";
    private const string DefaultContainerCpuCores = "0.25";
    private const string DefaultContainerMemoryGi = "0.5Gi";
    private const string DefaultTransportMethod = "auto";
    private const string EmptyParameterValue = "";
    private const string SystemManagedIdentityValue = "system";
    private const string AcrPasswordSecretNameValue = "acr-password";
    private const string CustomDomainBindingsExpression = "[for domain in customDomains: {\n  name: domain.domainName\n  bindingType: domain.bindingType\n}]";
    private const string AcrUsernameExpression = "split(acrLoginServer, '.')[0]";
    private const string ManagedIdentityClientIdConditionExpression = "!empty(acrManagedIdentityClientId)";
    private const string CustomDomainsConditionExpression = "!empty(customDomains)";
    private const string TransportMethodUnion = "'auto' | 'http' | 'http2' | 'tcp'";
    private const string CpuCoresPropertyName = "cpuCores";
    private const string MemoryGiPropertyName = "memoryGi";
    private const string MinReplicasPropertyName = "minReplicas";
    private const string MaxReplicasPropertyName = "maxReplicas";
    private const string EnabledPropertyName = "enabled";
    private const string TargetPortPropertyName = "targetPort";
    private const string ExternalPropertyName = "external";
    private const string TransportMethodPropertyName = "transportMethod";
    private const string IngressEnabledMappingKey = "ingressEnabled";
    private const string IngressTargetPortMappingKey = "ingressTargetPort";
    private const string IngressExternalMappingKey = "ingressExternal";
    private const string ReadinessProbePathMappingKey = "readinessProbePath";
    private const string ReadinessProbePortMappingKey = "readinessProbePort";
    private const string LivenessProbePathMappingKey = "livenessProbePath";
    private const string LivenessProbePortMappingKey = "livenessProbePort";
    private const string StartupProbePathMappingKey = "startupProbePath";
    private const string StartupProbePortMappingKey = "startupProbePort";
    private const string ReadinessPathSelector = "readiness.path";
    private const string ReadinessPortSelector = "readiness.port";
    private const string LivenessPathSelector = "liveness.path";
    private const string LivenessPortSelector = "liveness.port";
    private const string StartupPathSelector = "startup.path";
    private const string StartupPortSelector = "startup.port";
    private const string ValuePropertyName = "value";
    private const string SecretsPropertyName = "secrets";
    private const string RegistriesPropertyName = "registries";
    private const string ServerPropertyName = "server";
    private const string UsernamePropertyName = "username";
    private const string PasswordSecretRefPropertyName = "passwordSecretRef";
    private const string IdentityPropertyName = "identity";
    private const string IngressPropertyName = "ingress";
    private const string TransportPropertyName = "transport";
    private const string ContainersPropertyName = "containers";
    private const string ImagePropertyName = "image";
    private const string ResourcesPropertyName = "resources";
    private const string CpuPropertyName = "cpu";
    private const string MemoryPropertyName = "memory";
    private const string ProbesPropertyName = "probes";
    private const string ScalePropertyName = "scale";
    private const string ManagedEnvironmentIdPropertyName = "managedEnvironmentId";
    private const string ConfigurationPropertyName = "configuration";
    private const string TemplatePropertyName = "template";
    private const string NullExpression = "null";
    private const string ContainerRuntimeImageSelector = ContainerRuntimeParameterName + ".image";
    private const string ContainerRuntimeCpuJsonExpression = "json(" + ContainerRuntimeParameterName + "." + CpuCoresPropertyName + ")";
    private const string ContainerRuntimeMemorySelector = ContainerRuntimeParameterName + "." + MemoryGiPropertyName;
    private const string IngressEnabledSelector = IngressParameterName + "." + EnabledPropertyName;
    private const string IngressExternalSelector = IngressParameterName + "." + ExternalPropertyName;
    private const string IngressTargetPortSelector = IngressParameterName + "." + TargetPortPropertyName;
    private const string IngressTransportMethodSelector = IngressParameterName + "." + TransportMethodPropertyName;
    private const string ScalingMinReplicasSelector = ScalingParameterName + "." + MinReplicasPropertyName;
    private const string ScalingMaxReplicasSelector = ScalingParameterName + "." + MaxReplicasPropertyName;
    private const string FqdnOutputName = "fqdn";
    private const string LatestRevisionFqdnOutputName = "latestRevisionFqdn";
    private const string ContainerAppIdExpression = ResourceSymbol + ".id";
    private const string FqdnExpression = ResourceSymbol + ".properties.configuration.ingress != null ? " + ResourceSymbol + ".properties.configuration.ingress.fqdn : ''";
    private const string LatestRevisionFqdnExpression = ResourceSymbol + ".properties.latestRevisionFqdn";
    private const string ProbesUnionExpression = """
        union(
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
        """;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
      var containerRegistryId = resource.Properties.GetValueOrDefault(ContainerRegistryIdPropertyName, EmptyParameterValue);
        var hasAcr = !string.IsNullOrEmpty(containerRegistryId);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = hasAcr
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var builder = new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
          .Import(TypesImportPath,
                ContainerRuntimeConfigTypeName,
                ScalingConfigTypeName,
                IngressConfigTypeName,
                HealthProbeConfigTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Container App")
            .Param(NameParameterName, BicepType.String, "Name of the Container App")
            .Param(ContainerAppEnvironmentIdParameterName, BicepType.String, "Resource ID of the Container App Environment")
            .Param(ContainerRuntimeParameterName, BicepType.Custom(ContainerRuntimeConfigTypeName), "Container runtime configuration")
            .Param(ScalingParameterName, BicepType.Custom(ScalingConfigTypeName), "Scaling configuration")
            .Param(IngressParameterName, BicepType.Custom(IngressConfigTypeName), "Ingress configuration")
            .Param(HealthProbesParameterName, BicepType.Custom(HealthProbeConfigTypeName), "Health probe configuration");

        if (hasAcr)
        {
            builder.Param(AcrLoginServerParameterName, BicepType.String, "ACR login server (e.g. myregistry.azurecr.io)");

            if (useAdminCredentials)
            {
              builder.Param(AcrPasswordParameterName, BicepType.String,
                    "Admin password for the Container Registry", secure: true);
            }
            else
            {
              builder.Param(AcrManagedIdentityClientIdParameterName, BicepType.String,
                    "Client ID of the managed identity for ACR pull",
                defaultValue: new BicepStringLiteral(EmptyParameterValue));
            }
        }

          builder.Param(CustomDomainsParameterName, BicepType.Array, "Custom domain bindings for this Container App",
            defaultValue: new BicepArrayExpression([]));

        // ── Variables ──
          builder.Var(CustomDomainBindingsVariableName, new BicepRawExpression(CustomDomainBindingsExpression));

        if (hasAcr && useAdminCredentials)
        {
            builder.Var(AcrUsernameVariableName, new BicepRawExpression(AcrUsernameExpression));
            builder.Var(AcrPasswordSecretNameVariableName, new BicepStringLiteral(AcrPasswordSecretNameValue));
        }

        // ── Module file name (variant) ──
        if (hasAcr)
        {
            builder.ModuleFileName(useAdminCredentials
                ? AdminCredentialsModuleFileName
                : ManagedIdentityModuleFileName);
        }

        // ── Resource ──
        builder.Resource(ResourceSymbol, ContainerAppArmType)
          .Property(NamePropertyName, new BicepReference(NameParameterName))
          .Property(LocationPropertyName, new BicepReference(LocationParameterName));

        // configuration sub-object (variant-dependent)
        var configProps = new List<BicepPropertyAssignment>();

        if (hasAcr && useAdminCredentials)
        {
          configProps.Add(new BicepPropertyAssignment(SecretsPropertyName, new BicepArrayExpression([
                new BicepObjectExpression([
            new BicepPropertyAssignment(NamePropertyName, new BicepReference(AcrPasswordSecretNameVariableName)),
            new BicepPropertyAssignment(ValuePropertyName, new BicepReference(AcrPasswordParameterName)),
                ]),
            ])));
          configProps.Add(new BicepPropertyAssignment(RegistriesPropertyName, new BicepArrayExpression([
                new BicepObjectExpression([
            new BicepPropertyAssignment(ServerPropertyName, new BicepReference(AcrLoginServerParameterName)),
            new BicepPropertyAssignment(UsernamePropertyName, new BicepReference(AcrUsernameVariableName)),
            new BicepPropertyAssignment(PasswordSecretRefPropertyName, new BicepReference(AcrPasswordSecretNameVariableName)),
                ]),
            ])));
        }
        else if (hasAcr)
        {
          configProps.Add(new BicepPropertyAssignment(RegistriesPropertyName, new BicepArrayExpression([
                new BicepObjectExpression([
            new BicepPropertyAssignment(ServerPropertyName, new BicepReference(AcrLoginServerParameterName)),
              new BicepPropertyAssignment(IdentityPropertyName, new BicepConditionalExpression(
                  new BicepRawExpression(ManagedIdentityClientIdConditionExpression),
                  new BicepReference(AcrManagedIdentityClientIdParameterName),
                  new BicepStringLiteral(SystemManagedIdentityValue))),
                ]),
            ])));
        }

        configProps.Add(new BicepPropertyAssignment(IngressPropertyName, new BicepConditionalExpression(
          new BicepReference(IngressEnabledSelector),
            new BicepObjectExpression([
            new BicepPropertyAssignment(ExternalPropertyName, new BicepReference(IngressExternalSelector)),
            new BicepPropertyAssignment(TargetPortPropertyName, new BicepReference(IngressTargetPortSelector)),
            new BicepPropertyAssignment(TransportPropertyName, new BicepReference(IngressTransportMethodSelector)),
            new BicepPropertyAssignment(CustomDomainsParameterName, new BicepConditionalExpression(
                new BicepRawExpression(CustomDomainsConditionExpression),
                new BicepReference(CustomDomainBindingsVariableName),
              new BicepRawExpression(NullExpression))),
            ]),
          new BicepRawExpression(NullExpression))));

        // template sub-object
        var templateObject = new BicepObjectExpression([
          new BicepPropertyAssignment(ContainersPropertyName, new BicepArrayExpression([
                new BicepObjectExpression([
              new BicepPropertyAssignment(NamePropertyName, new BicepReference(NameParameterName)),
              new BicepPropertyAssignment(ImagePropertyName, new BicepReference(ContainerRuntimeImageSelector)),
              new BicepPropertyAssignment(ResourcesPropertyName, new BicepObjectExpression([
                new BicepPropertyAssignment(CpuPropertyName, new BicepRawExpression(ContainerRuntimeCpuJsonExpression)),
                new BicepPropertyAssignment(MemoryPropertyName, new BicepReference(ContainerRuntimeMemorySelector)),
                    ])),
              new BicepPropertyAssignment(ProbesPropertyName, new BicepRawExpression(BuildProbesUnion())),
                ]),
            ])),
          new BicepPropertyAssignment(ScalePropertyName, new BicepObjectExpression([
            new BicepPropertyAssignment(MinReplicasPropertyName, new BicepReference(ScalingMinReplicasSelector)),
            new BicepPropertyAssignment(MaxReplicasPropertyName, new BicepReference(ScalingMaxReplicasSelector)),
            ])),
        ]);

        builder.Property(PropertiesPropertyName, props => props
          .Property(ManagedEnvironmentIdPropertyName, new BicepReference(ContainerAppEnvironmentIdParameterName))
          .Property(ConfigurationPropertyName, new BicepObjectExpression(configProps))
          .Property(TemplatePropertyName, templateObject));

        // ── Outputs ──
        builder
          .Output(IdOutputName, BicepType.String, new BicepRawExpression(ContainerAppIdExpression),
                description: "The resource ID of the Container App")
          .Output(FqdnOutputName, BicepType.String, new BicepRawExpression(FqdnExpression),
                description: "The FQDN of the Container App")
          .Output(LatestRevisionFqdnOutputName, BicepType.String,
            new BicepRawExpression(LatestRevisionFqdnExpression),
                description: "The latest revision FQDN of the Container App");

        // ── Exported types ──
        builder
            .ExportedType(TransportMethodTypeName,
              new BicepRawExpression(TransportMethodUnion),
                description: "Ingress transport method for the Container App")
            .ExportedType(ContainerRuntimeConfigTypeName, new BicepRawExpression(
                "{\n  @description('Container image to deploy')\n  image: string\n  @description('CPU cores allocated to the container')\n  cpuCores: string\n  @description('Memory allocated to the container (e.g. 0.5Gi)')\n  memoryGi: string\n}"),
                description: "Container runtime configuration (image, CPU, memory)")
            .ExportedType(ScalingConfigTypeName, new BicepRawExpression(
                "{\n  @description('Minimum number of replicas')\n  minReplicas: int\n  @description('Maximum number of replicas')\n  maxReplicas: int\n}"),
                description: "Scaling configuration for the Container App")
            .ExportedType(IngressConfigTypeName, new BicepRawExpression(
              $"{{\n  @description('Whether ingress is enabled')\n  enabled: bool\n  @description('Target port for ingress traffic')\n  targetPort: int\n  @description('Whether ingress is externally accessible')\n  external: bool\n  @description('Transport method for ingress')\n  transportMethod: {TransportMethodTypeName}\n}}"),
                description: "Ingress configuration for the Container App")
            .ExportedType(ProbeConfigTypeName, new BicepRawExpression(
                "{\n  @description('HTTP path for the probe (empty to disable)')\n  path: string\n  @description('Port for the probe (0 to disable)')\n  port: int\n}"),
                description: "Configuration for a single HTTP health probe")
            .ExportedType(HealthProbeConfigTypeName, new BicepRawExpression(
              $"{{\n  @description('Readiness probe configuration')\n  readiness: {ProbeConfigTypeName}\n  @description('Liveness probe configuration')\n  liveness: {ProbeConfigTypeName}\n  @description('Startup probe configuration')\n  startup: {ProbeConfigTypeName}\n}}"),
                description: "Health probe configuration for the Container App");

        return builder.Build();
    }

    private static string BuildProbesUnion()
    {
        return ProbesUnionExpression;
    }

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerApp;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ContainerApp;

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
      var containerRegistryId = resource.Properties.GetValueOrDefault(ContainerRegistryIdPropertyName, EmptyParameterValue);
        var hasAcr = !string.IsNullOrEmpty(containerRegistryId);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = hasAcr
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);
        var hasCustomDomains = resource.CustomDomains.Count > 0;

      var dockerImageName = resource.Properties.GetValueOrDefault(DockerImageNamePropertyName, EmptyParameterValue);
        var containerImage = !string.IsNullOrEmpty(dockerImageName)
            ? dockerImageName
        : DefaultContainerImage;

        var parameters = new ContainerAppParameters
        {
          ContainerRuntime = new ContainerRuntimeParameters
            {
            Image = containerImage,
            CpuCores = DefaultContainerCpuCores,
            MemoryGi = DefaultContainerMemoryGi,
          },
          Scaling = new ScalingParameters
          {
            MinReplicas = 0,
            MaxReplicas = 1,
          },
          Ingress = new IngressParameters
          {
            Enabled = true,
            TargetPort = 80,
            External = true,
            TransportMethod = DefaultTransportMethod,
          },
          HealthProbes = new HealthProbesParameters
          {
            Readiness = new HealthProbeParameters { Path = string.Empty, Port = 0 },
            Liveness = new HealthProbeParameters { Path = string.Empty, Port = 0 },
            Startup = new HealthProbeParameters { Path = string.Empty, Port = 0 },
          },
        };

        if (hasAcr)
        {
          parameters = parameters with { AcrLoginServer = EmptyParameterValue };
          if (!useAdminCredentials)
          {
          parameters = parameters with { AcrManagedIdentityClientId = EmptyParameterValue };
          }
        }

        if (hasCustomDomains)
        {
          parameters = parameters with { CustomDomains = [] };
        }

        var moduleFileName = hasAcr
            ? useAdminCredentials
                ? AdminCredentialsModuleFileName
                : ManagedIdentityModuleFileName
            : ModuleName;

        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = moduleFileName,
            ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = hasAcr
                ? useAdminCredentials
                    ? ContainerAppWithAcrAdminCredentialsModuleTemplate
                    : ContainerAppWithAcrManagedIdentityModuleTemplate
                : ContainerAppModuleTemplate,
            ModuleTypesBicepContent = ContainerAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = BicepParameterModelConverter.ToDictionary(parameters),
            SecureParameters = useAdminCredentials ? [AcrPasswordParameterName] : [],
            ParameterTypeOverrides = new Dictionary<string, string>
            {
                [ContainerRuntimeParameterName] = ContainerRuntimeConfigTypeName,
                [ScalingParameterName] = ScalingConfigTypeName,
                [IngressParameterName] = IngressConfigTypeName,
                [HealthProbesParameterName] = HealthProbeConfigTypeName,
            },
            ParameterGroupMappings = new Dictionary<string, (string, string)>
            {
              [CpuCoresPropertyName] = (ContainerRuntimeParameterName, CpuCoresPropertyName),
              [MemoryGiPropertyName] = (ContainerRuntimeParameterName, MemoryGiPropertyName),
              [MinReplicasPropertyName] = (ScalingParameterName, MinReplicasPropertyName),
              [MaxReplicasPropertyName] = (ScalingParameterName, MaxReplicasPropertyName),
              [IngressEnabledMappingKey] = (IngressParameterName, EnabledPropertyName),
              [IngressTargetPortMappingKey] = (IngressParameterName, TargetPortPropertyName),
              [IngressExternalMappingKey] = (IngressParameterName, ExternalPropertyName),
              [TransportMethodPropertyName] = (IngressParameterName, TransportMethodPropertyName),
              [ReadinessProbePathMappingKey] = (HealthProbesParameterName, ReadinessPathSelector),
              [ReadinessProbePortMappingKey] = (HealthProbesParameterName, ReadinessPortSelector),
              [LivenessProbePathMappingKey] = (HealthProbesParameterName, LivenessPathSelector),
              [LivenessProbePortMappingKey] = (HealthProbesParameterName, LivenessPortSelector),
              [StartupProbePathMappingKey] = (HealthProbesParameterName, StartupPathSelector),
              [StartupProbePortMappingKey] = (HealthProbesParameterName, StartupPortSelector),
            }
        };
    }

    private static string GetAcrAuthMode(IReadOnlyDictionary<string, string> properties)
    {
      var acrAuthMode = properties.GetValueOrDefault(AcrAuthModePropertyName, string.Empty);
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
