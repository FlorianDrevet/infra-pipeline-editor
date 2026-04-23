using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep modules for Azure Web App resources with Code or Container deployment modes.</summary>
public sealed class WebAppTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
  private const string ManagedIdentityAcrAuthMode = "ManagedIdentity";
  private const string AdminCredentialsAcrAuthMode = "AdminCredentials";

    public string ResourceType
        => AzureResourceTypes.ArmTypes.WebApp;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.WebApp;

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var deploymentMode = resource.Properties.GetValueOrDefault("deploymentMode", "Code");
        var isContainer = string.Equals(deploymentMode, "Container", StringComparison.OrdinalIgnoreCase);
      var acrAuthMode = GetAcrAuthMode(resource.Properties);
      var useAdminCredentials = isContainer
        && string.Equals(acrAuthMode, AdminCredentialsAcrAuthMode, StringComparison.OrdinalIgnoreCase);

        var runtimeStack = resource.Properties.GetValueOrDefault("runtimeStack", "DOTNETCORE");
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
            ? "webAppContainerAdminCredentials"
            : "webAppContainerManagedIdentity"
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
          SecureParameters = isContainer && useAdminCredentials ? ["acrPassword"] : []
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
