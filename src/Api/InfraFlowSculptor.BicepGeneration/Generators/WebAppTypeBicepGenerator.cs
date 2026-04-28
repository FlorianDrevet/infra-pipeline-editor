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
    private const string ContainerKind = "app,linux,container";

    private const string ManagedIdentityModuleFileName = "webAppContainerManagedIdentity";
    private const string AdminCredentialsModuleFileName = "webAppContainerAdminCredentials";

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
        var deploymentMode = resource.Properties.GetValueOrDefault("deploymentMode", CodeDeploymentMode);
        var isContainer = string.Equals(deploymentMode, ContainerDeploymentMode, StringComparison.OrdinalIgnoreCase);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = isContainer
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var builder = new BicepModuleBuilder()
            .Module("webApp", "WebApp", ResourceTypeName)
            .Import("./types.bicep", "RuntimeStack")
            .Param("location", BicepType.String, "Azure region for the Web App")
            .Param("name", BicepType.String, "Name of the Web App")
            .Param("appServicePlanId", BicepType.String, "Resource ID of the App Service Plan")
            .Param("runtimeStack", BicepType.Custom("RuntimeStack"), "Runtime stack of the Web App",
                defaultValue: new BicepStringLiteral(DefaultRuntimeStack))
            .Param("runtimeVersion", BicepType.String, "Runtime version (e.g. 8.0, 18)")
            .Param("alwaysOn", BicepType.Bool, "Whether the app is always on")
            .Param("httpsOnly", BicepType.Bool, "Whether HTTPS only is enforced")
            .Param("deploymentMode", BicepType.String, "Deployment mode",
                defaultValue: new BicepStringLiteral(isContainer ? ContainerDeploymentMode : CodeDeploymentMode));

        // Container-specific params
        if (isContainer)
        {
            builder
                .Param("dockerImageName", BicepType.String, "Docker image name (e.g. myapp/api)")
                .Param("dockerImageTag", BicepType.String, "Docker image tag (e.g. latest, v1.2.3)",
                    defaultValue: new BicepStringLiteral("latest"))
                .Param("acrLoginServer", BicepType.String, "ACR login server (e.g. myregistry.azurecr.io)");

            if (useAdminCredentials)
            {
                builder.Param("acrPassword", BicepType.String,
                    "Admin password for the Container Registry", secure: true);
            }
            else
            {
                builder
                    .Param("acrUseManagedIdentityCreds", BicepType.Bool,
                        "Whether to use managed identity credentials for ACR",
                        defaultValue: new BicepBoolLiteral(true))
                    .Param("acrUserManagedIdentityId", BicepType.String,
                        "Client ID of the user-assigned managed identity for ACR pull",
                        defaultValue: new BicepStringLiteral(""));
            }
        }

        builder.Param("customDomains", BicepType.Array, "Custom domain bindings for this Web App",
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
        builder.Resource("webApp", "Microsoft.Web/sites@2023-12-01")
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
            new("alwaysOn", new BicepReference("alwaysOn")),
            new("ftpsState", new BicepStringLiteral("Disabled")),
            new("minTlsVersion", new BicepStringLiteral("1.2")),
        };

        if (isContainer && !useAdminCredentials)
        {
            siteConfigProps.Add(new BicepPropertyAssignment("acrUseManagedIdentityCreds",
                new BicepReference("acrUseManagedIdentityCreds")));
            siteConfigProps.Add(new BicepPropertyAssignment("acrUserManagedIdentityID",
                new BicepConditionalExpression(
                    new BicepRawExpression("!empty(acrUserManagedIdentityId)"),
                    new BicepReference("acrUserManagedIdentityId"),
                    new BicepRawExpression("null"))));
        }
        else if (isContainer && useAdminCredentials)
        {
            siteConfigProps.Add(new BicepPropertyAssignment("acrUseManagedIdentityCreds",
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
            .Property("httpsOnly", new BicepReference("httpsOnly"))
            .Property("siteConfig", new BicepObjectExpression(siteConfigProps)));

        // hostNameBindings for-loop child resource
        builder.AdditionalResource("hostNameBindings", "Microsoft.Web/sites/hostNameBindings@2023-12-01",
            forLoop: new BicepForLoop("domain", new BicepReference("customDomains")),
            parentSymbol: "webApp",
            bodyBuilder: body => body
                .Property("name", new BicepRawExpression("domain.domainName"))
                .Property("properties", p => p
                    .Property("siteName", new BicepRawExpression("webApp.name"))
                    .Property("hostNameType", new BicepStringLiteral("Verified"))
                    .Property("sslState", new BicepConditionalExpression(
                        new BicepRawExpression("domain.bindingType == 'SniEnabled'"),
                        new BicepStringLiteral("SniEnabled"),
                        new BicepStringLiteral("Disabled")))));

        // Outputs
        builder
            .Output("id", BicepType.String, new BicepRawExpression("webApp.id"),
                description: "The resource ID of the Web App")
            .Output("defaultHostName", BicepType.String,
                new BicepRawExpression("webApp.properties.defaultHostName"),
                description: "The default host name of the Web App")
            .Output("principalId", BicepType.String,
                new BicepRawExpression("webApp.identity.principalId"),
                description: "The principal ID of the system-assigned managed identity")
            .Output("customDomainVerificationId", BicepType.String,
                new BicepRawExpression("webApp.properties.customDomainVerificationId"),
                description: "The custom domain verification ID");

        // Exported types
        builder
            .ExportedType("RuntimeStack",
                new BicepRawExpression("'DOTNETCORE' | 'NODE' | 'PYTHON' | 'JAVA' | 'PHP'"),
                description: "Runtime stack for the Web App")
            .ExportedType("DeploymentMode",
                new BicepRawExpression("'Code' | 'Container'"),
                description: "Deployment mode for the Web App");

        return builder.Build();
    }

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var deploymentMode = resource.Properties.GetValueOrDefault("deploymentMode", CodeDeploymentMode);
        var isContainer = string.Equals(deploymentMode, ContainerDeploymentMode, StringComparison.OrdinalIgnoreCase);
        var acrAuthMode = GetAcrAuthMode(resource.Properties);
        var useAdminCredentials = isContainer
            && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var runtimeStack = resource.Properties.GetValueOrDefault("runtimeStack", DefaultRuntimeStack);
        var runtimeVersion = resource.Properties.GetValueOrDefault("runtimeVersion", "8.0");
        var alwaysOn = resource.Properties.GetValueOrDefault("alwaysOn", "true") == "true";
        var httpsOnly = resource.Properties.GetValueOrDefault("httpsOnly", "true") == "true";
        var dockerImageName = resource.Properties.GetValueOrDefault("dockerImageName", "");

        var parameters = new Dictionary<string, object>
        {
            ["runtimeStack"] = runtimeStack,
            ["runtimeVersion"] = runtimeVersion,
            ["alwaysOn"] = alwaysOn,
            ["httpsOnly"] = httpsOnly,
            ["deploymentMode"] = deploymentMode,
            ["customDomains"] = new List<object>(),
        };

        if (isContainer)
        {
            parameters["dockerImageName"] = dockerImageName;
            parameters["dockerImageTag"] = "latest";
            parameters["acrLoginServer"] = "";

            if (!useAdminCredentials)
            {
                parameters["acrUseManagedIdentityCreds"] = true;
                parameters["acrUserManagedIdentityId"] = "";
            }
        }

        var moduleFileName = isContainer
            ? useAdminCredentials
                ? AdminCredentialsModuleFileName
                : ManagedIdentityModuleFileName
            : "webApp";

        return new GeneratedTypeModule
        {
            ModuleName = "webApp",
            ModuleFileName = moduleFileName,
            ModuleFolderName = "WebApp",
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
        var acrAuthMode = properties.GetValueOrDefault("acrAuthMode", string.Empty);
        return string.IsNullOrWhiteSpace(acrAuthMode)
            ? ManagedIdentityAcrAuthMode
            : acrAuthMode;
    }

    private const string WebAppTypesTemplate = """
        @export()
        @description('Runtime stack for the Web App')
        type RuntimeStack = 'DOTNETCORE' | 'NODE' | 'PYTHON' | 'JAVA' | 'PHP'

        @export()
        @description('Deployment mode for the Web App')
        type DeploymentMode = 'Code' | 'Container'
        """;

    private const string WebAppCodeModuleTemplate = """
        import { RuntimeStack } from './types.bicep'

        @description('Azure region for the Web App')
        param location string

        @description('Name of the Web App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Web App')
        param runtimeStack RuntimeStack = 'DOTNETCORE'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether the app is always on')
        param alwaysOn bool

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Code'

        @description('Custom domain bindings for this Web App')
        param customDomains array = []

        var linuxFxVersion = '${toUpper(runtimeStack)}|${runtimeVersion}'

        resource webApp 'Microsoft.Web/sites@2023-12-01' = {
          name: name
          location: location
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: linuxFxVersion
              alwaysOn: alwaysOn
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
            }
          }
        }

        resource hostNameBindings 'Microsoft.Web/sites/hostNameBindings@2023-12-01' = [for domain in customDomains: {
          parent: webApp
          name: domain.domainName
          properties: {
            siteName: webApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Web App')
        output id string = webApp.id

        @description('The default host name of the Web App')
        output defaultHostName string = webApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = webApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = webApp.properties.customDomainVerificationId
        """;

    private const string WebAppContainerManagedIdentityModuleTemplate = """
        import { RuntimeStack } from './types.bicep'

        @description('Azure region for the Web App')
        param location string

        @description('Name of the Web App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Web App')
        param runtimeStack RuntimeStack = 'DOTNETCORE'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether the app is always on')
        param alwaysOn bool

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Container'

        @description('Docker image name (e.g. myapp/api)')
        param dockerImageName string

        @description('Docker image tag (e.g. latest, v1.2.3)')
        param dockerImageTag string = 'latest'

        @description('ACR login server (e.g. myregistry.azurecr.io)')
        param acrLoginServer string

        @description('Whether to use managed identity credentials for ACR')
        param acrUseManagedIdentityCreds bool = true

        @description('Client ID of the user-assigned managed identity for ACR pull')
        param acrUserManagedIdentityId string = ''

        @description('Custom domain bindings for this Web App')
        param customDomains array = []

        var dockerImage = '${acrLoginServer}/${dockerImageName}:${dockerImageTag}'

        resource webApp 'Microsoft.Web/sites@2023-12-01' = {
          name: name
          location: location
          kind: 'app,linux,container'
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: 'DOCKER|${dockerImage}'
              alwaysOn: alwaysOn
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
              acrUseManagedIdentityCreds: acrUseManagedIdentityCreds
              acrUserManagedIdentityID: !empty(acrUserManagedIdentityId) ? acrUserManagedIdentityId : null
            }
          }
        }

        resource hostNameBindings 'Microsoft.Web/sites/hostNameBindings@2023-12-01' = [for domain in customDomains: {
          parent: webApp
          name: domain.domainName
          properties: {
            siteName: webApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Web App')
        output id string = webApp.id

        @description('The default host name of the Web App')
        output defaultHostName string = webApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = webApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = webApp.properties.customDomainVerificationId
        """;

    private const string WebAppContainerAdminCredentialsModuleTemplate = """
        import { RuntimeStack } from './types.bicep'

        @description('Azure region for the Web App')
        param location string

        @description('Name of the Web App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Web App')
        param runtimeStack RuntimeStack = 'DOTNETCORE'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether the app is always on')
        param alwaysOn bool

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Container'

        @description('Docker image name (e.g. myapp/api)')
        param dockerImageName string

        @description('Docker image tag (e.g. latest, v1.2.3)')
        param dockerImageTag string = 'latest'

        @description('ACR login server (e.g. myregistry.azurecr.io)')
        param acrLoginServer string

        @secure()
        @description('Admin password for the Container Registry')
        param acrPassword string

        @description('Custom domain bindings for this Web App')
        param customDomains array = []

        var dockerImage = '${acrLoginServer}/${dockerImageName}:${dockerImageTag}'
        var acrUsername = split(acrLoginServer, '.')[0]

        resource webApp 'Microsoft.Web/sites@2023-12-01' = {
          name: name
          location: location
          kind: 'app,linux,container'
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: 'DOCKER|${dockerImage}'
              alwaysOn: alwaysOn
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
              acrUseManagedIdentityCreds: false
              appSettings: [
                {
                  name: 'DOCKER_REGISTRY_SERVER_URL'
                  value: 'https://${acrLoginServer}'
                }
                {
                  name: 'DOCKER_REGISTRY_SERVER_USERNAME'
                  value: acrUsername
                }
                {
                  name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
                  value: acrPassword
                }
              ]
            }
          }
        }

        resource hostNameBindings 'Microsoft.Web/sites/hostNameBindings@2023-12-01' = [for domain in customDomains: {
          parent: webApp
          name: domain.domainName
          properties: {
            siteName: webApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Web App')
        output id string = webApp.id

        @description('The default host name of the Web App')
        output defaultHostName string = webApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = webApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = webApp.properties.customDomainVerificationId
        """;
}
