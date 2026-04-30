using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep modules for Azure Web App resources with Code or Container deployment modes.</summary>
public sealed class WebAppTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
    private const string AdminCredentialsAcrAuthMode = "AdminCredentials";

    private const string CodeDeploymentMode = "Code";
    private const string ContainerDeploymentMode = "Container";
    private const string DefaultRuntimeStack = "DOTNETCORE";
    private const string DefaultRuntimeVersion = "8.0";
    private const string DefaultDockerImageTag = "latest";
    private const string BooleanTrueString = "true";
    private const string EmptyParameterValue = "";
    private const string ContainerKind = "app,linux,container";

    private const string ManagedIdentityModuleFileName = "webAppContainerManagedIdentity";
    private const string AdminCredentialsModuleFileName = "webAppContainerAdminCredentials";

    private const string RuntimeStackTypeName = "RuntimeStack";
    private const string DeploymentModeTypeName = "DeploymentMode";
    private const string DeploymentModePropertyName = "deploymentMode";
    private const string RuntimeStackPropertyName = "runtimeStack";
    private const string RuntimeVersionPropertyName = "runtimeVersion";
    private const string AlwaysOnPropertyName = "alwaysOn";
    private const string HttpsOnlyPropertyName = "httpsOnly";
    private const string DockerImageNamePropertyName = "dockerImageName";
    private const string DockerImageTagPropertyName = "dockerImageTag";
    private const string AcrLoginServerPropertyName = "acrLoginServer";
    private const string AcrAuthModePropertyName = "acrAuthMode";
    private const string AcrUseManagedIdentityCredsPropertyName = "acrUseManagedIdentityCreds";
    private const string AcrUserManagedIdentityIdPropertyName = "acrUserManagedIdentityId";
    private const string CustomDomainsParameterName = "customDomains";
    private const string WebAppModuleName = "webApp";
    private const string WebAppModuleFolderName = "WebApp";
    private const string WebAppArmType = "Microsoft.Web/sites@2023-12-01";
    private const string HostNameBindingsArmType = "Microsoft.Web/sites/hostNameBindings@2023-12-01";

    private const string DockerRegistryServerUrlSettingName = "DOCKER_REGISTRY_SERVER_URL";
    private const string DockerRegistryServerUsernameSettingName = "DOCKER_REGISTRY_SERVER_USERNAME";
    private const string DockerRegistryServerPasswordSettingName = "DOCKER_REGISTRY_SERVER_PASSWORD";

    public string ResourceType
        => AzureResourceTypes.ArmTypes.WebApp;

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
            .Import("./types.bicep", RuntimeStackTypeName)
            .Param("location", BicepType.String, "Azure region for the Web App")
            .Param("name", BicepType.String, "Name of the Web App")
            .Param("appServicePlanId", BicepType.String, "Resource ID of the App Service Plan")
            .Param(RuntimeStackPropertyName, BicepType.Custom(RuntimeStackTypeName), "Runtime stack of the Web App",
                defaultValue: new BicepStringLiteral(DefaultRuntimeStack))
            .Param(RuntimeVersionPropertyName, BicepType.String, "Runtime version (e.g. 8.0, 18)")
            .Param(AlwaysOnPropertyName, BicepType.Bool, "Whether the app is always on")
            .Param(HttpsOnlyPropertyName, BicepType.Bool, "Whether HTTPS only is enforced")
            .Param(DeploymentModePropertyName, BicepType.String, "Deployment mode",
                defaultValue: new BicepStringLiteral(isContainer ? ContainerDeploymentMode : CodeDeploymentMode));

        // Container-specific params
        if (isContainer)
        {
            builder
                .Param(DockerImageNamePropertyName, BicepType.String, "Docker image name (e.g. myapp/api)")
                .Param(DockerImageTagPropertyName, BicepType.String, "Docker image tag (e.g. latest, v1.2.3)",
                    defaultValue: new BicepStringLiteral(DefaultDockerImageTag))
                .Param(AcrLoginServerPropertyName, BicepType.String, "ACR login server (e.g. myregistry.azurecr.io)");

            if (useAdminCredentials)
            {
                builder.Param("acrPassword", BicepType.String,
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

        builder.Param(CustomDomainsParameterName, BicepType.Array, "Custom domain bindings for this Web App",
            defaultValue: new BicepArrayExpression([]));

        // Variables
        if (isContainer)
        {
            builder.Var("dockerImage", new BicepRawExpression("'${acrLoginServer}/${dockerImageName}:${dockerImageTag}'"));
            if (useAdminCredentials)
            {
                builder.Var("acrUsername", new BicepRawExpression("split(acrLoginServer, '.')[0]"));
            }

            // Module file name for container variants
            builder.ModuleFileName(useAdminCredentials
                ? AdminCredentialsModuleFileName
                : ManagedIdentityModuleFileName);
        }
        else
        {
            builder.Var("linuxFxVersion", new BicepRawExpression("'${toUpper(runtimeStack)}|${runtimeVersion}'"));
        }

        // Primary resource
        builder.Resource(WebAppModuleName, WebAppArmType)
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"));

        if (isContainer)
        {
            builder.Property("kind", new BicepStringLiteral(ContainerKind));
        }

        // Build siteConfig properties
        var siteConfigProps = new List<BicepPropertyAssignment>
        {
            new("linuxFxVersion", isContainer
                ? new BicepRawExpression("'DOCKER|${dockerImage}'")
                : new BicepReference("linuxFxVersion")),
            new(AlwaysOnPropertyName, new BicepReference(AlwaysOnPropertyName)),
            new("ftpsState", new BicepStringLiteral("Disabled")),
            new("minTlsVersion", new BicepStringLiteral("1.2")),
        };

        if (isContainer && !useAdminCredentials)
        {
            siteConfigProps.Add(new BicepPropertyAssignment(AcrUseManagedIdentityCredsPropertyName,
                new BicepReference(AcrUseManagedIdentityCredsPropertyName)));
            siteConfigProps.Add(new BicepPropertyAssignment("acrUserManagedIdentityID",
                new BicepConditionalExpression(
                    new BicepRawExpression($"!empty({AcrUserManagedIdentityIdPropertyName})"),
                    new BicepReference(AcrUserManagedIdentityIdPropertyName),
                    new BicepRawExpression("null"))));
        }
        else if (isContainer && useAdminCredentials)
        {
            siteConfigProps.Add(new BicepPropertyAssignment(AcrUseManagedIdentityCredsPropertyName,
                new BicepBoolLiteral(false)));
            siteConfigProps.Add(new BicepPropertyAssignment("appSettings",
                new BicepArrayExpression([
                    new BicepObjectExpression([
                        new BicepPropertyAssignment("name", new BicepStringLiteral(DockerRegistryServerUrlSettingName)),
                        new BicepPropertyAssignment("value", new BicepRawExpression("'https://${acrLoginServer}'")),
                    ]),
                    new BicepObjectExpression([
                        new BicepPropertyAssignment("name", new BicepStringLiteral(DockerRegistryServerUsernameSettingName)),
                        new BicepPropertyAssignment("value", new BicepReference("acrUsername")),
                    ]),
                    new BicepObjectExpression([
                        new BicepPropertyAssignment("name", new BicepStringLiteral(DockerRegistryServerPasswordSettingName)),
                        new BicepPropertyAssignment("value", new BicepReference("acrPassword")),
                    ]),
                ])));
        }

        builder.Property("properties", props => props
            .Property("serverFarmId", new BicepReference("appServicePlanId"))
            .Property(HttpsOnlyPropertyName, new BicepReference(HttpsOnlyPropertyName))
            .Property("siteConfig", new BicepObjectExpression(siteConfigProps)));

        // hostNameBindings for-loop child resource
        builder.AdditionalResource("hostNameBindings", HostNameBindingsArmType,
            forLoop: new BicepForLoop("domain", new BicepReference(CustomDomainsParameterName)),
            parentSymbol: WebAppModuleName,
            bodyBuilder: body => body
                .Property("name", new BicepRawExpression("domain.domainName"))
                .Property("properties", p => p
                    .Property("siteName", new BicepRawExpression($"{WebAppModuleName}.name"))
                    .Property("hostNameType", new BicepStringLiteral("Verified"))
                    .Property("sslState", new BicepConditionalExpression(
                        new BicepRawExpression("domain.bindingType == 'SniEnabled'"),
                        new BicepStringLiteral("SniEnabled"),
                        new BicepStringLiteral("Disabled")))));

        // Outputs
        builder
            .Output("id", BicepType.String, new BicepRawExpression($"{WebAppModuleName}.id"),
                description: "The resource ID of the Web App")
            .Output("defaultHostName", BicepType.String,
                new BicepRawExpression($"{WebAppModuleName}.properties.defaultHostName"),
                description: "The default host name of the Web App")
            .Output("principalId", BicepType.String,
                new BicepRawExpression($"{WebAppModuleName}.identity.principalId"),
                description: "The principal ID of the system-assigned managed identity")
            .Output("customDomainVerificationId", BicepType.String,
                new BicepRawExpression($"{WebAppModuleName}.properties.customDomainVerificationId"),
                description: "The custom domain verification ID");

        // Exported types
        builder
            .ExportedType(RuntimeStackTypeName,
                new BicepRawExpression("'DOTNETCORE' | 'NODE' | 'PYTHON' | 'JAVA' | 'PHP'"),
                description: "Runtime stack for the Web App")
            .ExportedType(DeploymentModeTypeName,
                new BicepRawExpression($"'{CodeDeploymentMode}' | '{ContainerDeploymentMode}'"),
                description: "Deployment mode for the Web App");

        return builder.Build();
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

        var parameters = new Dictionary<string, object>
        {
            [RuntimeStackPropertyName] = runtimeStack,
            [RuntimeVersionPropertyName] = runtimeVersion,
            [AlwaysOnPropertyName] = alwaysOn,
            [HttpsOnlyPropertyName] = httpsOnly,
            [DeploymentModePropertyName] = deploymentMode,
            [CustomDomainsParameterName] = new List<object>(),
        };

        if (isContainer)
        {
            parameters[DockerImageNamePropertyName] = dockerImageName;
            parameters[DockerImageTagPropertyName] = DefaultDockerImageTag;
            parameters[AcrLoginServerPropertyName] = EmptyParameterValue;

            if (!useAdminCredentials)
            {
                parameters[AcrUseManagedIdentityCredsPropertyName] = true;
                parameters[AcrUserManagedIdentityIdPropertyName] = EmptyParameterValue;
            }
        }

        var moduleFileName = isContainer
            ? useAdminCredentials
                ? AdminCredentialsModuleFileName
                : ManagedIdentityModuleFileName
            : "webApp";

        return new GeneratedTypeModule
        {
            ModuleName = WebAppModuleName,
            ModuleFileName = moduleFileName,
            ModuleFolderName = WebAppModuleFolderName,
            ModuleBicepContent = isContainer
                ? useAdminCredentials
                    ? WebAppContainerAdminCredentialsModuleTemplate
                    : WebAppContainerManagedIdentityModuleTemplate
                : WebAppCodeModuleTemplate,
            ModuleTypesBicepContent = WebAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = parameters,
            SecureParameters = isContainer && useAdminCredentials ? ["acrPassword"] : [],
        };
    }

    private static string GetAcrAuthMode(IReadOnlyDictionary<string, string> properties)
    {
        var acrAuthMode = properties.GetValueOrDefault(AcrAuthModePropertyName, string.Empty);
        return string.IsNullOrWhiteSpace(acrAuthMode)
            ? ManagedIdentityAcrAuthMode
            : acrAuthMode;
    }

    private static readonly string WebAppTypesTemplate = $$"""
        @export()
        @description('Runtime stack for the Web App')
        type {{RuntimeStackTypeName}} = '{{DefaultRuntimeStack}}' | 'NODE' | 'PYTHON' | 'JAVA' | 'PHP'

        @export()
        @description('Deployment mode for the Web App')
        type {{DeploymentModeTypeName}} = '{{CodeDeploymentMode}}' | '{{ContainerDeploymentMode}}'
        """;

    private static readonly string WebAppCodeModuleTemplate = $$"""
        import { {{RuntimeStackTypeName}} } from './types.bicep'

        @description('Azure region for the Web App')
        param location string

        @description('Name of the Web App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Web App')
                param {{RuntimeStackPropertyName}} {{RuntimeStackTypeName}} = '{{DefaultRuntimeStack}}'

        @description('Runtime version (e.g. 8.0, 18)')
                param {{RuntimeVersionPropertyName}} string

        @description('Whether the app is always on')
                param {{AlwaysOnPropertyName}} bool

        @description('Whether HTTPS only is enforced')
                param {{HttpsOnlyPropertyName}} bool

        @description('Deployment mode')
                param {{DeploymentModePropertyName}} string = '{{CodeDeploymentMode}}'

        @description('Custom domain bindings for this Web App')
                param {{CustomDomainsParameterName}} array = []

                var linuxFxVersion = '${toUpper({{RuntimeStackPropertyName}})}|${{{RuntimeVersionPropertyName}}}'

                resource {{WebAppModuleName}} '{{WebAppArmType}}' = {
          name: name
          location: location
          properties: {
            serverFarmId: appServicePlanId
                        {{HttpsOnlyPropertyName}}: {{HttpsOnlyPropertyName}}
            siteConfig: {
              linuxFxVersion: linuxFxVersion
                            {{AlwaysOnPropertyName}}: {{AlwaysOnPropertyName}}
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
            }
          }
        }

                resource hostNameBindings '{{HostNameBindingsArmType}}' = [for domain in {{CustomDomainsParameterName}}: {
                    parent: {{WebAppModuleName}}
          name: domain.domainName
          properties: {
                        siteName: {{WebAppModuleName}}.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Web App')
                output id string = {{WebAppModuleName}}.id

        @description('The default host name of the Web App')
                output defaultHostName string = {{WebAppModuleName}}.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
                output principalId string = {{WebAppModuleName}}.identity.principalId

        @description('The custom domain verification ID')
                output customDomainVerificationId string = {{WebAppModuleName}}.properties.customDomainVerificationId
                """;

        private static readonly string WebAppContainerManagedIdentityModuleTemplate = $$"""
                import { {{RuntimeStackTypeName}} } from './types.bicep'

        @description('Azure region for the Web App')
        param location string

        @description('Name of the Web App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Web App')
                param {{RuntimeStackPropertyName}} {{RuntimeStackTypeName}} = '{{DefaultRuntimeStack}}'

        @description('Runtime version (e.g. 8.0, 18)')
                param {{RuntimeVersionPropertyName}} string

        @description('Whether the app is always on')
                param {{AlwaysOnPropertyName}} bool

        @description('Whether HTTPS only is enforced')
                param {{HttpsOnlyPropertyName}} bool

        @description('Deployment mode')
                param {{DeploymentModePropertyName}} string = '{{ContainerDeploymentMode}}'

        @description('Docker image name (e.g. myapp/api)')
                param {{DockerImageNamePropertyName}} string

        @description('Docker image tag (e.g. latest, v1.2.3)')
                param {{DockerImageTagPropertyName}} string = '{{DefaultDockerImageTag}}'

        @description('ACR login server (e.g. myregistry.azurecr.io)')
                param {{AcrLoginServerPropertyName}} string

        @description('Whether to use managed identity credentials for ACR')
                param {{AcrUseManagedIdentityCredsPropertyName}} bool = true

        @description('Client ID of the user-assigned managed identity for ACR pull')
                param {{AcrUserManagedIdentityIdPropertyName}} string = '{{EmptyParameterValue}}'

        @description('Custom domain bindings for this Web App')
                param {{CustomDomainsParameterName}} array = []

                var dockerImage = '${{{AcrLoginServerPropertyName}}}/${{{DockerImageNamePropertyName}}}:${{{DockerImageTagPropertyName}}}'

                resource {{WebAppModuleName}} '{{WebAppArmType}}' = {
          name: name
          location: location
                    kind: '{{ContainerKind}}'
          properties: {
            serverFarmId: appServicePlanId
                        {{HttpsOnlyPropertyName}}: {{HttpsOnlyPropertyName}}
            siteConfig: {
              linuxFxVersion: 'DOCKER|${dockerImage}'
                            {{AlwaysOnPropertyName}}: {{AlwaysOnPropertyName}}
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
                            {{AcrUseManagedIdentityCredsPropertyName}}: {{AcrUseManagedIdentityCredsPropertyName}}
                            acrUserManagedIdentityID: !empty({{AcrUserManagedIdentityIdPropertyName}}) ? {{AcrUserManagedIdentityIdPropertyName}} : null
            }
          }
        }

                resource hostNameBindings '{{HostNameBindingsArmType}}' = [for domain in {{CustomDomainsParameterName}}: {
                    parent: {{WebAppModuleName}}
          name: domain.domainName
          properties: {
                        siteName: {{WebAppModuleName}}.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Web App')
                output id string = {{WebAppModuleName}}.id

        @description('The default host name of the Web App')
                output defaultHostName string = {{WebAppModuleName}}.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
                output principalId string = {{WebAppModuleName}}.identity.principalId

        @description('The custom domain verification ID')
                output customDomainVerificationId string = {{WebAppModuleName}}.properties.customDomainVerificationId
                """;

        private static readonly string WebAppContainerAdminCredentialsModuleTemplate = $$"""
                import { {{RuntimeStackTypeName}} } from './types.bicep'

        @description('Azure region for the Web App')
        param location string

        @description('Name of the Web App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Web App')
                param {{RuntimeStackPropertyName}} {{RuntimeStackTypeName}} = '{{DefaultRuntimeStack}}'

        @description('Runtime version (e.g. 8.0, 18)')
                param {{RuntimeVersionPropertyName}} string

        @description('Whether the app is always on')
                param {{AlwaysOnPropertyName}} bool

        @description('Whether HTTPS only is enforced')
                param {{HttpsOnlyPropertyName}} bool

        @description('Deployment mode')
                param {{DeploymentModePropertyName}} string = '{{ContainerDeploymentMode}}'

        @description('Docker image name (e.g. myapp/api)')
                param {{DockerImageNamePropertyName}} string

        @description('Docker image tag (e.g. latest, v1.2.3)')
                param {{DockerImageTagPropertyName}} string = '{{DefaultDockerImageTag}}'

        @description('ACR login server (e.g. myregistry.azurecr.io)')
                param {{AcrLoginServerPropertyName}} string

        @secure()
        @description('Admin password for the Container Registry')
        param acrPassword string

        @description('Custom domain bindings for this Web App')
                param {{CustomDomainsParameterName}} array = []

                var dockerImage = '${{{AcrLoginServerPropertyName}}}/${{{DockerImageNamePropertyName}}}:${{{DockerImageTagPropertyName}}}'
                var acrUsername = split({{AcrLoginServerPropertyName}}, '.')[0]

                resource {{WebAppModuleName}} '{{WebAppArmType}}' = {
          name: name
          location: location
                    kind: '{{ContainerKind}}'
          properties: {
            serverFarmId: appServicePlanId
                        {{HttpsOnlyPropertyName}}: {{HttpsOnlyPropertyName}}
            siteConfig: {
              linuxFxVersion: 'DOCKER|${dockerImage}'
                            {{AlwaysOnPropertyName}}: {{AlwaysOnPropertyName}}
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
                            {{AcrUseManagedIdentityCredsPropertyName}}: false
              appSettings: [
                {
                                    name: '{{DockerRegistryServerUrlSettingName}}'
                                    value: 'https://${{{AcrLoginServerPropertyName}}}'
                }
                {
                                    name: '{{DockerRegistryServerUsernameSettingName}}'
                  value: acrUsername
                }
                {
                                    name: '{{DockerRegistryServerPasswordSettingName}}'
                  value: acrPassword
                }
              ]
            }
          }
        }

                resource hostNameBindings '{{HostNameBindingsArmType}}' = [for domain in {{CustomDomainsParameterName}}: {
                    parent: {{WebAppModuleName}}
          name: domain.domainName
          properties: {
                        siteName: {{WebAppModuleName}}.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Web App')
                output id string = {{WebAppModuleName}}.id

        @description('The default host name of the Web App')
                output defaultHostName string = {{WebAppModuleName}}.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
                output principalId string = {{WebAppModuleName}}.identity.principalId

        @description('The custom domain verification ID')
                output customDomainVerificationId string = {{WebAppModuleName}}.properties.customDomainVerificationId
                """;
}
