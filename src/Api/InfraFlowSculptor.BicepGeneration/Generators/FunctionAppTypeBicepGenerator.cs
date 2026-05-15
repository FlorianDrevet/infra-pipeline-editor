using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep modules for Azure Function App resources with Code or Container deployment modes.</summary>
public sealed partial class FunctionAppTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
  private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
  private const string AdminCredentialsAcrAuthMode = "AdminCredentials";
  private const string ModuleName = "functionApp";
  private const string ModuleFolderName = "FunctionApp";
  private const string RuntimeStackTypeName = "RuntimeStack";
  private const string WorkerRuntimeTypeName = "WorkerRuntime";
  private const string DeploymentModeTypeName = "DeploymentMode";
  private const string AcrAuthModePropertyName = "acrAuthMode";
  private const string AppServicePlanIdParameterName = "appServicePlanId";
  private const string RuntimeStackParameterName = "runtimeStack";
  private const string RuntimeVersionParameterName = "runtimeVersion";
  private const string HttpsOnlyParameterName = "httpsOnly";
  private const string DeploymentModeParameterName = "deploymentMode";
  private const string DockerImageNameParameterName = "dockerImageName";
  private const string DockerImageTagParameterName = "dockerImageTag";
  private const string AcrLoginServerParameterName = "acrLoginServer";
  private const string AcrPasswordParameterName = "acrPassword";
  private const string AcrUseManagedIdentityCredsParameterName = "acrUseManagedIdentityCreds";
  private const string AcrUserManagedIdentityIdParameterName = "acrUserManagedIdentityId";
  private const string CustomDomainsParameterName = "customDomains";
  private const string DockerImageVariableName = "dockerImage";
  private const string WorkerRuntimeVariableName = "workerRuntime";
  private const string AcrUsernameVariableName = "acrUsername";
  private const string LinuxFxVersionVariableName = "linuxFxVersion";
  private const string FunctionAppResourceSymbol = "functionApp";
  private const string HostNameBindingsResourceName = "hostNameBindings";
  private const string FunctionAppArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.WebAppArmType;
  private const string HostNameBindingsArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.HostNameBindingsArmType;
  private const string CodeDeploymentMode = "Code";
  private const string ContainerDeploymentMode = "Container";
  private const string DefaultRuntimeStack = "DOTNET";
  private const string DefaultDockerImageTag = "latest";
  private const string EmptyParameterValue = "";
  private const string ManagedIdentityModuleFileName = "functionAppContainerManagedIdentity";
  private const string AdminCredentialsModuleFileName = "functionAppContainerAdminCredentials";
  private const string FunctionAppCodeKind = "functionapp";
  private const string FunctionAppContainerKind = "functionapp,linux,container";
  private const string FtpsStatePropertyName = "ftpsState";
  private const string MinTlsVersionPropertyName = "minTlsVersion";
  private const string MinimumTlsVersionValue = "1.2";
  private const string DisabledStateValue = "Disabled";
  private const string SiteConfigPropertyName = "siteConfig";
  private const string AppSettingsPropertyName = "appSettings";
  private const string ServerFarmIdPropertyName = "serverFarmId";
  private const string AcrUserManagedIdentityIdSiteConfigPropertyName = "acrUserManagedIdentityID";
  private const string HostNameTypePropertyName = "hostNameType";
  private const string HostNameTypeVerifiedValue = "Verified";
  private const string SiteNamePropertyName = "siteName";
  private const string SslStatePropertyName = "sslState";
  private const string SniEnabledBindingTypeValue = "SniEnabled";
  private const string FunctionsWorkerRuntimeSettingName = "FUNCTIONS_WORKER_RUNTIME";
  private const string FunctionsExtensionVersionSettingName = "FUNCTIONS_EXTENSION_VERSION";
  private const string FunctionsExtensionVersionValue = "~4";
  private const string DockerRegistryServerUrlSettingName = "DOCKER_REGISTRY_SERVER_URL";
  private const string DockerRegistryServerUsernameSettingName = "DOCKER_REGISTRY_SERVER_USERNAME";
  private const string DockerRegistryServerPasswordSettingName = "DOCKER_REGISTRY_SERVER_PASSWORD";
  private const string DockerImageExpression = "'${acrLoginServer}/${dockerImageName}:${dockerImageTag}'";
  private const string AcrUsernameExpression = "split(acrLoginServer, '.')[0]";
  private const string LinuxFxVersionExpression = "'${toUpper(runtimeStack)}|${runtimeVersion}'";
  private const string DockerLinuxFxVersionExpression = "'DOCKER|${dockerImage}'";
  private const string AcrLoginServerUrlExpression = "'https://${acrLoginServer}'";
  private const string WorkerRuntimeExpression = "toUpper(runtimeStack) == 'DOTNET' ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet') : toLower(runtimeStack)";
  private const string RuntimeStackUnion = "'DOTNET' | 'NODE' | 'PYTHON' | 'JAVA' | 'POWERSHELL'";
  private const string WorkerRuntimeUnion = "'dotnet' | 'dotnet-isolated' | 'node' | 'python' | 'java' | 'powershell'";
  private const string DeploymentModeUnion = "'Code' | 'Container'";
  private const string ValuePropertyName = "value";
  private const string LinuxFxVersionPropertyName = "linuxFxVersion";
  private const string DomainLoopVariableName = "domain";
  private const string DomainNameExpression = DomainLoopVariableName + ".domainName";
  private const string DomainBindingTypeSniEnabledExpression = DomainLoopVariableName + ".bindingType == '" + SniEnabledBindingTypeValue + "'";
  private const string AcrUserManagedIdentityIdNotEmptyExpression = "!empty(" + AcrUserManagedIdentityIdParameterName + ")";
  private const string NullExpression = "null";
  private const string FunctionAppNameExpression = FunctionAppResourceSymbol + ".name";
  private const string DefaultHostNameOutputName = "defaultHostName";
  private const string PrincipalIdOutputName = "principalId";
  private const string CustomDomainVerificationIdOutputName = "customDomainVerificationId";
  private const string FunctionAppIdExpression = FunctionAppResourceSymbol + ".id";
  private const string DefaultHostNameExpression = FunctionAppResourceSymbol + ".properties.defaultHostName";
  private const string PrincipalIdExpression = FunctionAppResourceSymbol + ".identity.principalId";
  private const string CustomDomainVerificationIdExpression = FunctionAppResourceSymbol + ".properties.customDomainVerificationId";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.FunctionAppType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.FunctionApp;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
      var deploymentMode = resource.Properties.GetValueOrDefault(DeploymentModeParameterName, CodeDeploymentMode);
      var isContainer = string.Equals(deploymentMode, ContainerDeploymentMode, StringComparison.OrdinalIgnoreCase);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = isContainer
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var builder = new BicepModuleBuilder()
        .Module(ModuleName, ModuleFolderName, ResourceTypeName)
        .Import(TypesImportPath, RuntimeStackTypeName, WorkerRuntimeTypeName)
        .Param(LocationParameterName, BicepType.String, "Azure region for the Function App")
        .Param(NameParameterName, BicepType.String, "Name of the Function App")
        .Param(AppServicePlanIdParameterName, BicepType.String, "Resource ID of the App Service Plan")
        .Param(RuntimeStackParameterName, BicepType.Custom(RuntimeStackTypeName), "Runtime stack of the Function App",
          defaultValue: new BicepStringLiteral(DefaultRuntimeStack))
        .Param(RuntimeVersionParameterName, BicepType.String, "Runtime version (e.g. 8.0, 18)")
        .Param(HttpsOnlyParameterName, BicepType.Bool, "Whether HTTPS only is enforced")
        .Param(DeploymentModeParameterName, BicepType.String, "Deployment mode",
          defaultValue: new BicepStringLiteral(isContainer ? ContainerDeploymentMode : CodeDeploymentMode));

        AddContainerParameters(builder, isContainer, useAdminCredentials);

          builder.Param(CustomDomainsParameterName, BicepType.Array, "Custom domain bindings for this Function App",
            defaultValue: new BicepArrayExpression([]));

        AddVariables(builder, isContainer, useAdminCredentials);

        var functionsAppSettings = BuildFunctionsAppSettings(isContainer, useAdminCredentials);
        var siteConfigProps = BuildSiteConfigProperties(isContainer, useAdminCredentials, functionsAppSettings);

        // Primary resource
        builder.Resource(FunctionAppResourceSymbol, FunctionAppArmType)
          .Property(NamePropertyName, new BicepReference(NameParameterName))
          .Property(LocationPropertyName, new BicepReference(LocationParameterName))
          .Property(KindPropertyName, new BicepStringLiteral(isContainer ? FunctionAppContainerKind : FunctionAppCodeKind))
            .Property(PropertiesPropertyName, props => props
            .Property(ServerFarmIdPropertyName, new BicepReference(AppServicePlanIdParameterName))
            .Property(HttpsOnlyParameterName, new BicepReference(HttpsOnlyParameterName))
            .Property(SiteConfigPropertyName, new BicepObjectExpression(siteConfigProps)));

        AddHostNameBindings(builder);
        AddOutputs(builder);
        AddExportedTypes(builder);

        return builder.Build();
    }

    private static void AddContainerParameters(BicepModuleBuilder builder, bool isContainer, bool useAdminCredentials)
    {
        if (!isContainer)
            return;

        builder
          .Param(DockerImageNameParameterName, BicepType.String, "Docker image name (e.g. myapp/functions)")
          .Param(DockerImageTagParameterName, BicepType.String, "Docker image tag (e.g. latest, v1.2.3)",
            defaultValue: new BicepStringLiteral(DefaultDockerImageTag))
          .Param(AcrLoginServerParameterName, BicepType.String, "ACR login server (e.g. myregistry.azurecr.io)");

        if (useAdminCredentials)
        {
            builder.Param(AcrPasswordParameterName, BicepType.String,
                "Admin password for the Container Registry", secure: true);
            return;
        }

        builder
            .Param(AcrUseManagedIdentityCredsParameterName, BicepType.Bool,
                "Whether to use managed identity credentials for ACR",
                defaultValue: new BicepBoolLiteral(true))
            .Param(AcrUserManagedIdentityIdParameterName, BicepType.String,
                "Client ID of the user-assigned managed identity for ACR pull",
                defaultValue: new BicepStringLiteral(EmptyParameterValue));
    }

    private static void AddVariables(BicepModuleBuilder builder, bool isContainer, bool useAdminCredentials)
    {
        var workerRuntimeExpr = new BicepRawExpression(WorkerRuntimeExpression);

        if (!isContainer)
        {
            builder.Var(LinuxFxVersionVariableName, new BicepRawExpression(LinuxFxVersionExpression));
            builder.Var(WorkerRuntimeVariableName, workerRuntimeExpr);
            return;
        }

        builder.Var(DockerImageVariableName, new BicepRawExpression(DockerImageExpression));
        builder.Var(WorkerRuntimeVariableName, workerRuntimeExpr);
        if (useAdminCredentials)
        {
            builder.Var(AcrUsernameVariableName, new BicepRawExpression(AcrUsernameExpression));
        }

        builder.ModuleFileName(useAdminCredentials
            ? AdminCredentialsModuleFileName
            : ManagedIdentityModuleFileName);
    }

    private static List<BicepExpression> BuildFunctionsAppSettings(bool isContainer, bool useAdminCredentials)
    {
        var functionsAppSettings = new List<BicepExpression>
        {
            new BicepObjectExpression([
                new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral(FunctionsWorkerRuntimeSettingName)),
                new BicepPropertyAssignment(ValuePropertyName, new BicepReference(WorkerRuntimeVariableName)),
            ]),
            new BicepObjectExpression([
                new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral(FunctionsExtensionVersionSettingName)),
                new BicepPropertyAssignment(ValuePropertyName, new BicepStringLiteral(FunctionsExtensionVersionValue)),
            ]),
        };

        if (!isContainer || !useAdminCredentials)
            return functionsAppSettings;

        functionsAppSettings.Add(new BicepObjectExpression([
            new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral(DockerRegistryServerUrlSettingName)),
            new BicepPropertyAssignment(ValuePropertyName, new BicepRawExpression(AcrLoginServerUrlExpression)),
        ]));
        functionsAppSettings.Add(new BicepObjectExpression([
            new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral(DockerRegistryServerUsernameSettingName)),
            new BicepPropertyAssignment(ValuePropertyName, new BicepReference(AcrUsernameVariableName)),
        ]));
        functionsAppSettings.Add(new BicepObjectExpression([
            new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral(DockerRegistryServerPasswordSettingName)),
            new BicepPropertyAssignment(ValuePropertyName, new BicepReference(AcrPasswordParameterName)),
        ]));
        return functionsAppSettings;
    }

    private static List<BicepPropertyAssignment> BuildSiteConfigProperties(
        bool isContainer,
        bool useAdminCredentials,
        List<BicepExpression> functionsAppSettings)
    {
        var siteConfigProps = new List<BicepPropertyAssignment>
        {
            new(LinuxFxVersionPropertyName, isContainer
                ? new BicepRawExpression(DockerLinuxFxVersionExpression)
                : new BicepReference(LinuxFxVersionVariableName)),
            new(FtpsStatePropertyName, new BicepStringLiteral(DisabledStateValue)),
            new(MinTlsVersionPropertyName, new BicepStringLiteral(MinimumTlsVersionValue)),
        };

        if (isContainer && !useAdminCredentials)
        {
            siteConfigProps.Add(new BicepPropertyAssignment(AcrUseManagedIdentityCredsParameterName,
                new BicepReference(AcrUseManagedIdentityCredsParameterName)));
            siteConfigProps.Add(new BicepPropertyAssignment(AcrUserManagedIdentityIdSiteConfigPropertyName,
                new BicepConditionalExpression(
                    new BicepRawExpression(AcrUserManagedIdentityIdNotEmptyExpression),
                    new BicepReference(AcrUserManagedIdentityIdParameterName),
                    new BicepRawExpression(NullExpression))));
        }
              else if (useAdminCredentials)
        {
            siteConfigProps.Add(new BicepPropertyAssignment(AcrUseManagedIdentityCredsParameterName,
                new BicepBoolLiteral(false)));
        }

        siteConfigProps.Add(new BicepPropertyAssignment(AppSettingsPropertyName,
            new BicepArrayExpression(functionsAppSettings)));
        return siteConfigProps;
    }

    private static void AddHostNameBindings(BicepModuleBuilder builder)
    {
        builder.AdditionalResource(HostNameBindingsResourceName, HostNameBindingsArmType,
            forLoop: new BicepForLoop(DomainLoopVariableName, new BicepReference(CustomDomainsParameterName)),
            parentSymbol: FunctionAppResourceSymbol,
            bodyBuilder: body => body
                .Property(NamePropertyName, new BicepRawExpression(DomainNameExpression))
                .Property(PropertiesPropertyName, p => p
                    .Property(SiteNamePropertyName, new BicepRawExpression(FunctionAppNameExpression))
                    .Property(HostNameTypePropertyName, new BicepStringLiteral(HostNameTypeVerifiedValue))
                    .Property(SslStatePropertyName, new BicepConditionalExpression(
                        new BicepRawExpression(DomainBindingTypeSniEnabledExpression),
                        new BicepStringLiteral(SniEnabledBindingTypeValue),
                        new BicepStringLiteral(DisabledStateValue)))));
    }

    private static void AddOutputs(BicepModuleBuilder builder)
    {
        builder
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(FunctionAppIdExpression),
                description: "The resource ID of the Function App")
            .Output(DefaultHostNameOutputName, BicepType.String,
                new BicepRawExpression(DefaultHostNameExpression),
                description: "The default host name of the Function App")
            .Output(PrincipalIdOutputName, BicepType.String,
                new BicepRawExpression(PrincipalIdExpression),
                description: "The principal ID of the system-assigned managed identity")
            .Output(CustomDomainVerificationIdOutputName, BicepType.String,
                new BicepRawExpression(CustomDomainVerificationIdExpression),
                description: "The custom domain verification ID");
    }

    private static void AddExportedTypes(BicepModuleBuilder builder)
    {
        builder
            .ExportedType(RuntimeStackTypeName,
                new BicepRawExpression(RuntimeStackUnion),
                description: "Runtime stack for the Function App")
            .ExportedType(WorkerRuntimeTypeName,
                new BicepRawExpression(WorkerRuntimeUnion),
                description: "Functions worker runtime identifier")
            .ExportedType(DeploymentModeTypeName,
                new BicepRawExpression(DeploymentModeUnion),
                description: "Deployment mode for the Function App");
    }

    private static string GetAcrAuthMode(IReadOnlyDictionary<string, string> properties)
    {
      var acrAuthMode = properties.GetValueOrDefault(AcrAuthModePropertyName, string.Empty);
        return string.IsNullOrWhiteSpace(acrAuthMode)
            ? ManagedIdentityAcrAuthMode
            : acrAuthMode;
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var deploymentMode = resource.Properties.GetValueOrDefault(DeploymentModeParameterName, CodeDeploymentMode);
        var isContainer = string.Equals(deploymentMode, ContainerDeploymentMode, StringComparison.OrdinalIgnoreCase);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = isContainer
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var moduleFileName = ModuleName;
        var moduleBicepContent = FunctionAppCodeModuleTemplate;

        if (isContainer)
        {
            moduleFileName = useAdminCredentials
                ? AdminCredentialsModuleFileName
                : ManagedIdentityModuleFileName;
            moduleBicepContent = useAdminCredentials
                ? FunctionAppContainerAdminCredentialsModuleTemplate
                : FunctionAppContainerManagedIdentityModuleTemplate;
        }

        return new GeneratedTypeModule
        {
          ModuleName = ModuleName,
            ModuleFileName = moduleFileName,
          ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = moduleBicepContent,
            ModuleTypesBicepContent = FunctionAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
          SecureParameters = isContainer && useAdminCredentials ? [AcrPasswordParameterName] : [],
        };
    }

}
