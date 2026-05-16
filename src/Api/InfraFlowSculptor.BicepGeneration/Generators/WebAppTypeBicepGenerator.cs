using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep modules for Azure Web App resources with Code or Container deployment modes.</summary>
public sealed partial class WebAppTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
    private const string AdminCredentialsAcrAuthMode = "AdminCredentials";

    private const string CodeDeploymentMode = "Code";
    private const string ContainerDeploymentMode = "Container";
    private const string DefaultRuntimeStack = "DOTNETCORE";
    private const string DefaultRuntimeVersion = "8.0";
    private const string DefaultDockerImageTag = "latest";
    private const string EmptyParameterValue = "";
    private const string ContainerKind = "app,linux,container";

    private const string ManagedIdentityModuleFileName = "webAppContainerManagedIdentity";
    private const string AdminCredentialsModuleFileName = "webAppContainerAdminCredentials";

    private const string RuntimeStackTypeName = "RuntimeStack";
    private const string DeploymentModeTypeName = "DeploymentMode";
    private const string AppServicePlanIdParameterName = "appServicePlanId";
    private const string DeploymentModePropertyName = "deploymentMode";
    private const string RuntimeStackPropertyName = "runtimeStack";
    private const string RuntimeVersionPropertyName = "runtimeVersion";
    private const string AlwaysOnPropertyName = "alwaysOn";
    private const string HttpsOnlyPropertyName = "httpsOnly";
    private const string DockerImageNamePropertyName = "dockerImageName";
    private const string DockerImageValidatedPropertyName = "dockerImageValidated";
    private const string DockerImageTagPropertyName = "dockerImageTag";
    private const string AcrLoginServerPropertyName = "acrLoginServer";
    private const string AcrAuthModePropertyName = "acrAuthMode";
    private const string AcrUseManagedIdentityCredsPropertyName = "acrUseManagedIdentityCreds";
    private const string AcrUserManagedIdentityIdPropertyName = "acrUserManagedIdentityId";
    private const string AcrUserManagedIdentityIdSiteConfigPropertyName = "acrUserManagedIdentityID";
    private const string AcrPasswordParameterName = "acrPassword";
    private const string CustomDomainsParameterName = "customDomains";
    private const string WebAppModuleName = "webApp";
    private const string WebAppModuleFolderName = "WebApp";
    private const string WebAppArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.WebAppArmType;
    private const string HostNameBindingsArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.HostNameBindingsArmType;
    private const string HostNameBindingsResourceName = "hostNameBindings";
    private const string DockerImageVariableName = "dockerImage";
    private const string AcrUsernameVariableName = "acrUsername";
    private const string LinuxFxVersionVariableName = "linuxFxVersion";
    private const string SiteConfigPropertyName = "siteConfig";
    private const string AppSettingsPropertyName = "appSettings";
    private const string FtpsStatePropertyName = "ftpsState";
    private const string DisabledStateValue = "Disabled";
    private const string MinTlsVersionPropertyName = "minTlsVersion";
    private const string MinimumTlsVersionValue = "1.2";
    private const string ServerFarmIdPropertyName = "serverFarmId";
    private const string LinuxFxVersionPropertyName = "linuxFxVersion";
    private const string HostNameTypePropertyName = "hostNameType";
    private const string HostNameTypeVerifiedValue = "Verified";
    private const string SslStatePropertyName = "sslState";
    private const string SniEnabledBindingTypeValue = "SniEnabled";
    private const string SiteNamePropertyName = "siteName";
    private const string ValuePropertyName = "value";
    private const string DomainLoopVariableName = "domain";
    private const string DomainNameExpression = DomainLoopVariableName + ".domainName";
    private const string DomainBindingTypeSniEnabledExpression = DomainLoopVariableName + ".bindingType == '" + SniEnabledBindingTypeValue + "'";
    private const string NullExpression = "null";
    private const string DockerImageExpression = "'${acrLoginServer}/${dockerImageName}:${dockerImageTag}'";
    private const string AcrUsernameExpression = "split(acrLoginServer, '.')[0]";
    private const string LinuxFxVersionExpression = "'${toUpper(runtimeStack)}|${runtimeVersion}'";
    private const string ContainerLinuxFxVersionExpression = "'DOCKER|${dockerImage}'";
    private const string AcrLoginServerUrlExpression = "'https://${acrLoginServer}'";
    private const string DefaultHostNameOutputName = "defaultHostName";
    private const string PrincipalIdOutputName = "principalId";
    private const string CustomDomainVerificationIdOutputName = "customDomainVerificationId";
    private const string WebAppIdExpression = WebAppModuleName + ".id";
    private const string WebAppDefaultHostNameExpression = WebAppModuleName + ".properties.defaultHostName";
    private const string WebAppPrincipalIdExpression = WebAppModuleName + ".identity.principalId";
    private const string WebAppCustomDomainVerificationIdExpression = WebAppModuleName + ".properties.customDomainVerificationId";
    private const string WebAppNameExpression = WebAppModuleName + ".name";
    private const string RuntimeStackUnionExpression = "'DOTNETCORE' | 'NODE' | 'PYTHON' | 'JAVA' | 'PHP'";
    private const string DeploymentModeUnionExpression = "'Code' | 'Container'";

    private const string DockerRegistryServerUrlSettingName = "DOCKER_REGISTRY_SERVER_URL";
    private const string DockerRegistryServerUsernameSettingName = "DOCKER_REGISTRY_SERVER_USERNAME";
    private const string DockerRegistryServerPasswordSettingName = "DOCKER_REGISTRY_SERVER_PASSWORD";

    public string ResourceType
        => AzureResourceTypes.ArmTypes.WebAppType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.WebApp;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        var deploymentMode = resource.Properties.GetValueOrDefault(DeploymentModePropertyName, CodeDeploymentMode);
        var isContainer = string.Equals(deploymentMode, ContainerDeploymentMode, StringComparison.OrdinalIgnoreCase);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = isContainer
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var builder = new BicepModuleBuilder()
            .Module(WebAppModuleName, WebAppModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, RuntimeStackTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Web App")
            .Param(NameParameterName, BicepType.String, "Name of the Web App")
            .Param(AppServicePlanIdParameterName, BicepType.String, "Resource ID of the App Service Plan")
            .Param(RuntimeStackPropertyName, BicepType.Custom(RuntimeStackTypeName), "Runtime stack of the Web App",
                defaultValue: new BicepStringLiteral(DefaultRuntimeStack))
            .Param(RuntimeVersionPropertyName, BicepType.String, "Runtime version (e.g. 8.0, 18)")
            .Param(AlwaysOnPropertyName, BicepType.Bool, "Whether the app is always on")
            .Param(HttpsOnlyPropertyName, BicepType.Bool, "Whether HTTPS only is enforced")
            .Param(DeploymentModePropertyName, BicepType.String, "Deployment mode",
                defaultValue: new BicepStringLiteral(isContainer ? ContainerDeploymentMode : CodeDeploymentMode));

        AddContainerParameters(builder, isContainer, useAdminCredentials);

        builder.Param(CustomDomainsParameterName, BicepType.Array, "Custom domain bindings for this Web App",
            defaultValue: new BicepArrayExpression([]));

        AddVariables(builder, isContainer, useAdminCredentials);

        // Primary resource
        builder.Resource(WebAppModuleName, WebAppArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName));

        if (isContainer)
        {
            builder.Property(KindPropertyName, new BicepStringLiteral(ContainerKind));
        }

        builder.Property(PropertiesPropertyName, props => props
            .Property(ServerFarmIdPropertyName, new BicepReference(AppServicePlanIdParameterName))
            .Property(HttpsOnlyPropertyName, new BicepReference(HttpsOnlyPropertyName))
            .Property(SiteConfigPropertyName, new BicepObjectExpression(
                BuildSiteConfigProperties(isContainer, useAdminCredentials))));

        AddHostNameBindings(builder);
        AddOutputs(builder);
        AddExportedTypes(builder);

        return builder.Build();
    }

    private static void AddContainerParameters(BicepModuleBuilder builder, bool isContainer, bool useAdminCredentials)
    {
        if (!isContainer)
        {
            return;
        }

        builder
            .Param(DockerImageNamePropertyName, BicepType.String, "Docker image name (e.g. myapp/api)")
            .Param(DockerImageTagPropertyName, BicepType.String, "Docker image tag (e.g. latest, v1.2.3)",
                defaultValue: new BicepStringLiteral(DefaultDockerImageTag))
            .Param(AcrLoginServerPropertyName, BicepType.String, "ACR login server (e.g. myregistry.azurecr.io)");

        if (useAdminCredentials)
        {
            builder.Param(AcrPasswordParameterName, BicepType.String,
                "Admin password for the Container Registry", secure: true);
        }
        else
        {
            builder
                .Param(AcrUseManagedIdentityCredsPropertyName, BicepType.Bool,
                    "Whether to use managed identity credentials for ACR",
                    defaultValue: new BicepBoolLiteral(true))
                .Param(AcrUserManagedIdentityIdPropertyName, BicepType.String,
                    "Client ID of the user-assigned managed identity for ACR pull",
                    defaultValue: new BicepStringLiteral(EmptyParameterValue));
        }
    }

    private static void AddVariables(BicepModuleBuilder builder, bool isContainer, bool useAdminCredentials)
    {
        if (!isContainer)
        {
            builder.Var(LinuxFxVersionVariableName, new BicepRawExpression(LinuxFxVersionExpression));
            return;
        }

        builder.Var(DockerImageVariableName, new BicepRawExpression(DockerImageExpression));
        if (useAdminCredentials)
        {
            builder.Var(AcrUsernameVariableName, new BicepRawExpression(AcrUsernameExpression));
        }

        builder.ModuleFileName(useAdminCredentials
            ? AdminCredentialsModuleFileName
            : ManagedIdentityModuleFileName);
    }

    private static List<BicepPropertyAssignment> BuildSiteConfigProperties(bool isContainer, bool useAdminCredentials)
    {
        var props = new List<BicepPropertyAssignment>
        {
            new(LinuxFxVersionPropertyName, isContainer
                ? new BicepRawExpression(ContainerLinuxFxVersionExpression)
                : new BicepReference(LinuxFxVersionVariableName)),
            new(AlwaysOnPropertyName, new BicepReference(AlwaysOnPropertyName)),
            new(FtpsStatePropertyName, new BicepStringLiteral(DisabledStateValue)),
            new(MinTlsVersionPropertyName, new BicepStringLiteral(MinimumTlsVersionValue)),
        };

        if (isContainer && !useAdminCredentials)
        {
            props.Add(new BicepPropertyAssignment(AcrUseManagedIdentityCredsPropertyName,
                new BicepReference(AcrUseManagedIdentityCredsPropertyName)));
            props.Add(new BicepPropertyAssignment(AcrUserManagedIdentityIdSiteConfigPropertyName,
                new BicepConditionalExpression(
                    new BicepRawExpression($"!empty({AcrUserManagedIdentityIdPropertyName})"),
                    new BicepReference(AcrUserManagedIdentityIdPropertyName),
                    new BicepRawExpression(NullExpression))));
        }
        else if (useAdminCredentials)
        {
            props.Add(new BicepPropertyAssignment(AcrUseManagedIdentityCredsPropertyName,
                new BicepBoolLiteral(false)));
            props.Add(new BicepPropertyAssignment(AppSettingsPropertyName,
                new BicepArrayExpression([
                    new BicepObjectExpression([
                        new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral(DockerRegistryServerUrlSettingName)),
                        new BicepPropertyAssignment(ValuePropertyName, new BicepRawExpression(AcrLoginServerUrlExpression)),
                    ]),
                    new BicepObjectExpression([
                        new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral(DockerRegistryServerUsernameSettingName)),
                        new BicepPropertyAssignment(ValuePropertyName, new BicepReference(AcrUsernameVariableName)),
                    ]),
                    new BicepObjectExpression([
                        new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral(DockerRegistryServerPasswordSettingName)),
                        new BicepPropertyAssignment(ValuePropertyName, new BicepReference(AcrPasswordParameterName)),
                    ]),
                ])));
        }

        return props;
    }

    private static void AddHostNameBindings(BicepModuleBuilder builder)
    {
        builder.AdditionalResource(HostNameBindingsResourceName, HostNameBindingsArmType,
            forLoop: new BicepForLoop(DomainLoopVariableName, new BicepReference(CustomDomainsParameterName)),
            parentSymbol: WebAppModuleName,
            bodyBuilder: body => body
                .Property(NamePropertyName, new BicepRawExpression(DomainNameExpression))
                .Property(PropertiesPropertyName, p => p
                    .Property(SiteNamePropertyName, new BicepRawExpression(WebAppNameExpression))
                    .Property(HostNameTypePropertyName, new BicepStringLiteral(HostNameTypeVerifiedValue))
                    .Property(SslStatePropertyName, new BicepConditionalExpression(
                        new BicepRawExpression(DomainBindingTypeSniEnabledExpression),
                        new BicepStringLiteral(SniEnabledBindingTypeValue),
                        new BicepStringLiteral(DisabledStateValue)))));
    }

    private static void AddOutputs(BicepModuleBuilder builder)
    {
        builder
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(WebAppIdExpression),
                description: "The resource ID of the Web App")
            .Output(DefaultHostNameOutputName, BicepType.String,
                new BicepRawExpression(WebAppDefaultHostNameExpression),
                description: "The default host name of the Web App")
            .Output(PrincipalIdOutputName, BicepType.String,
                new BicepRawExpression(WebAppPrincipalIdExpression),
                description: "The principal ID of the system-assigned managed identity")
            .Output(CustomDomainVerificationIdOutputName, BicepType.String,
                new BicepRawExpression(WebAppCustomDomainVerificationIdExpression),
                description: "The custom domain verification ID");
    }

    private static void AddExportedTypes(BicepModuleBuilder builder)
    {
        builder
            .ExportedType(RuntimeStackTypeName,
                new BicepRawExpression(RuntimeStackUnionExpression),
                description: "Runtime stack for the Web App")
            .ExportedType(DeploymentModeTypeName,
                new BicepRawExpression(DeploymentModeUnionExpression),
                description: "Deployment mode for the Web App");
    }

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var deploymentMode = resource.Properties.GetValueOrDefault(DeploymentModePropertyName, CodeDeploymentMode);
        var isContainer = string.Equals(deploymentMode, ContainerDeploymentMode, StringComparison.OrdinalIgnoreCase);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = isContainer
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var runtimeStack = resource.Properties.GetValueOrDefault(RuntimeStackPropertyName, DefaultRuntimeStack);
        var runtimeVersion = resource.Properties.GetValueOrDefault(RuntimeVersionPropertyName, DefaultRuntimeVersion);
        var alwaysOn = resource.Properties.GetValueOrDefault(AlwaysOnPropertyName, BooleanTrueString) == BooleanTrueString;
        var httpsOnly = resource.Properties.GetValueOrDefault(HttpsOnlyPropertyName, BooleanTrueString) == BooleanTrueString;
        var dockerImageName = resource.Properties.GetValueOrDefault(DockerImageNamePropertyName, EmptyParameterValue);
        var dockerImageValidated = string.Equals(
            resource.Properties.GetValueOrDefault(DockerImageValidatedPropertyName, EmptyParameterValue),
            BooleanTrueString,
            StringComparison.OrdinalIgnoreCase);

        var parameters = new WebAppParameters
        {
            RuntimeStack = runtimeStack,
            RuntimeVersion = runtimeVersion,
            AlwaysOn = alwaysOn,
            HttpsOnly = httpsOnly,
            DeploymentMode = deploymentMode,
            CustomDomains = [],
        };

        if (isContainer)
        {
            parameters = parameters with
            {
                DockerImageName = !string.IsNullOrEmpty(dockerImageName) && dockerImageValidated ? dockerImageName : EmptyParameterValue,
                DockerImageTag = DefaultDockerImageTag,
                AcrLoginServer = EmptyParameterValue,
            };

            if (!useAdminCredentials)
            {
                parameters = parameters with
                {
                    AcrUseManagedIdentityCreds = true,
                    AcrUserManagedIdentityId = EmptyParameterValue,
                };
            }
        }

        var moduleFileName = WebAppModuleName;
        var moduleBicepContent = WebAppCodeModuleTemplate;

        if (isContainer)
        {
            moduleFileName = useAdminCredentials
                ? AdminCredentialsModuleFileName
                : ManagedIdentityModuleFileName;
            moduleBicepContent = useAdminCredentials
                ? WebAppContainerAdminCredentialsModuleTemplate
                : WebAppContainerManagedIdentityModuleTemplate;
        }

        return new GeneratedTypeModule
        {
            ModuleName = WebAppModuleName,
            ModuleFileName = moduleFileName,
            ModuleFolderName = WebAppModuleFolderName,
            ModuleBicepContent = moduleBicepContent,
            ModuleTypesBicepContent = WebAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = BicepParameterModelConverter.ToDictionary(parameters),
            SecureParameters = isContainer && useAdminCredentials ? [AcrPasswordParameterName] : [],
        };
    }

    private static string GetAcrAuthMode(IReadOnlyDictionary<string, string> properties)
    {
        var acrAuthMode = properties.GetValueOrDefault(AcrAuthModePropertyName, string.Empty);
        return string.IsNullOrWhiteSpace(acrAuthMode)
            ? ManagedIdentityAcrAuthMode
            : acrAuthMode;
    }
}
