using InfraFlowSculptor.BicepGeneration.Generators.Helpers;
using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels.ContainerApp;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;
using static InfraFlowSculptor.BicepGeneration.Generators.Helpers.ContainerAppAcrBicepHelper;
using static InfraFlowSculptor.BicepGeneration.Generators.Helpers.ContainerAppTypesBicepHelper;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container App (<c>Microsoft.App/containerApps</c>).
/// Supports optional ACR integration with managed identity or admin credentials-based image pulling.
/// </summary>
public sealed partial class ContainerAppTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
    private const string AdminCredentialsAcrAuthMode = "AdminCredentials";

    private const string ModuleName = "containerApp";
    private const string ModuleFolderName = "ContainerApp";
    private const string AdminCredentialsModuleFileName = "containerAppAcrAdminCredentials";
    private const string ResourceSymbol = "containerApp";

    private const string ContainerRuntimeParameterName = "containerRuntime";
    private const string ScalingParameterName = "scaling";
    private const string IngressParameterName = "ingress";
    private const string HealthProbesParameterName = "healthProbes";

    private const string AcrAuthModePropertyName = "acrAuthMode";
    private const string ContainerRegistryIdPropertyName = "containerRegistryId";
    private const string DockerImageNamePropertyName = "dockerImageName";
    private const string DockerImageValidatedPropertyName = "dockerImageValidated";
    private const string ContainerAppEnvironmentIdParameterName = "containerAppEnvironmentId";
    private const string CustomDomainsParameterName = "customDomains";
    private const string CustomDomainBindingsVariableName = "customDomainBindings";
    private const string ContainerAppArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.ContainerAppArmType;
    private const string ContainerImageParameterName = "containerImage";
    private const string DefaultContainerImage = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest";
    private const string DefaultContainerCpuCores = "0.25";
    private const string DefaultContainerMemoryGi = "0.5Gi";
    private const string DefaultTransportMethod = "auto";
    private const string EmptyParameterValue = "";
    private const string ValidatedDnsValidationStatus = "Validated";
    private const string CustomDomainBindingsExpression = "[for domain in customDomains: {\n  name: domain.domainName\n  bindingType: domain.bindingType\n}]";
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
    private const string ManagedEnvironmentIdPropertyName = "managedEnvironmentId";
    private const string ConfigurationPropertyName = "configuration";
    private const string TemplatePropertyName = "template";

    private const string FqdnOutputName = "fqdn";
    private const string LatestRevisionFqdnOutputName = "latestRevisionFqdn";
    private const string ContainerAppIdExpression = ResourceSymbol + ".id";
    private const string FqdnExpression = ResourceSymbol + ".properties.configuration.ingress != null ? " + ResourceSymbol + ".properties.configuration.ingress.fqdn : ''";
    private const string LatestRevisionFqdnExpression = ResourceSymbol + ".properties.latestRevisionFqdn";

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
      var containerRegistryId = resource.Properties.GetValueOrDefault(ContainerRegistryIdPropertyName, EmptyParameterValue);
        var hasAcr = !string.IsNullOrEmpty(containerRegistryId);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = hasAcr
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);
        var hasValidatedCustomDomains = HasValidatedCustomDomains(resource);

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
            .Param(ContainerImageParameterName, BicepType.String, "Container image (overridden by app pipeline after first deploy)",
                defaultValue: new BicepStringLiteral(DefaultContainerImage))
            .Param(ContainerRuntimeParameterName, BicepType.Custom(ContainerRuntimeConfigTypeName), "Container runtime configuration")
            .Param(ScalingParameterName, BicepType.Custom(ScalingConfigTypeName), "Scaling configuration")
            .Param(IngressParameterName, BicepType.Custom(IngressConfigTypeName), "Ingress configuration")
            .Param(HealthProbesParameterName, BicepType.Custom(HealthProbeConfigTypeName), "Health probe configuration");

        if (hasAcr)
        {
            builder.AddAcrParameters(useAdminCredentials);
        }

        if (hasValidatedCustomDomains)
        {
          builder.Param(CustomDomainsParameterName, BicepType.Array, "Custom domain bindings for this Container App",
            defaultValue: new BicepArrayExpression([]));
            builder.Var(CustomDomainBindingsVariableName, new BicepRawExpression(CustomDomainBindingsExpression));
        }

        // ── Variables ──

        if (hasAcr && useAdminCredentials)
        {
            builder.AddAcrAdminVariables();
        }

        if (hasAcr && useAdminCredentials)
        {
          builder.ModuleFileName(AdminCredentialsModuleFileName);
        }
        else if (hasAcr)
        {
          builder.ModuleFileName(ModuleName);
        }

        // ── Resource ──
        builder.Resource(ResourceSymbol, ContainerAppArmType)
          .Property(NamePropertyName, new BicepReference(NameParameterName))
          .Property(LocationPropertyName, new BicepReference(LocationParameterName));

        // configuration sub-object (variant-dependent)
        var configProps = hasAcr
            ? ContainerAppAcrBicepHelper.BuildAcrConfigProperties(useAdminCredentials)
            : new List<BicepPropertyAssignment>();

        configProps.Add(ContainerAppSpecBicepHelper.BuildIngressConfigProperty(hasValidatedCustomDomains));

        // template sub-object
        var templateObject = ContainerAppSpecBicepHelper.BuildTemplateObject(
            NameParameterName, ContainerImageParameterName);

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
        builder.AddContainerAppExportedTypes();

        return builder.Build();
    }

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerAppType;

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
        var hasValidatedCustomDomains = HasValidatedCustomDomains(resource);

      var dockerImageName = resource.Properties.GetValueOrDefault(DockerImageNamePropertyName, EmptyParameterValue);
      var dockerImageValidated = string.Equals(
          resource.Properties.GetValueOrDefault(DockerImageValidatedPropertyName, EmptyParameterValue),
          "true", StringComparison.OrdinalIgnoreCase);

        var parameters = new ContainerAppParameters
        {
          ContainerImage = !string.IsNullOrEmpty(dockerImageName) && dockerImageValidated ? dockerImageName : null,
          ContainerRuntime = new ContainerRuntimeParameters
            {
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

        if (hasValidatedCustomDomains)
        {
          parameters = parameters with { CustomDomains = [] };
        }

        var moduleFileName = hasAcr && useAdminCredentials
            ? AdminCredentialsModuleFileName
            : ModuleName;

        var moduleBicepContent = ContainerAppModuleTemplate;
        if (hasAcr)
        {
          moduleBicepContent = useAdminCredentials
            ? ContainerAppWithAcrAdminCredentialsModuleTemplate
            : ContainerAppWithAcrManagedIdentityModuleTemplate;
        }

        moduleBicepContent = ApplyCustomDomainSupport(moduleBicepContent, hasValidatedCustomDomains);

        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = moduleFileName,
            ModuleFolderName = ModuleFolderName,
          ModuleBicepContent = moduleBicepContent,
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

    private static bool HasValidatedCustomDomains(ResourceDefinition resource)
    {
      return resource.CustomDomains.Any(customDomain =>
        customDomain.DnsValidationStatus.Equals(ValidatedDnsValidationStatus, StringComparison.OrdinalIgnoreCase));
    }

    private static string ApplyCustomDomainSupport(string template, bool hasValidatedCustomDomains)
    {
      return template
        .Replace(
          CustomDomainDeclarationsPlaceholder,
          hasValidatedCustomDomains ? CustomDomainDeclarationsBlock : string.Empty,
          StringComparison.Ordinal)
        .Replace(
          IngressCustomDomainsPropertyPlaceholder,
          hasValidatedCustomDomains ? IngressCustomDomainsPropertyBlock : string.Empty,
          StringComparison.Ordinal);
    }

}
