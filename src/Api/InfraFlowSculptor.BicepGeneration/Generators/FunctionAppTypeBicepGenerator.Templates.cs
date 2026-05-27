namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed partial class FunctionAppTypeBicepGenerator
{
    private const string FunctionAppTypesTemplate = """
        @export()
        @description('Runtime stack for the Function App')
        type RuntimeStack = 'DOTNET' | 'NODE' | 'PYTHON' | 'JAVA' | 'POWERSHELL'

        @export()
        @description('Functions worker runtime identifier')
        type WorkerRuntime = 'dotnet' | 'dotnet-isolated' | 'node' | 'python' | 'java' | 'powershell'

        @export()
        @description('Deployment mode for the Function App')
        type DeploymentMode = 'Code' | 'Container'
        """;

    private static readonly string FunctionAppCodeModuleTemplate = $$"""
        import { RuntimeStack, WorkerRuntime } from './types.bicep'

        @description('Azure region for the Function App')
        param location string

        @description('Name of the Function App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Function App')
        param runtimeStack RuntimeStack = 'DOTNET'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Code'

        @description('Custom domain bindings for this Function App')
        param customDomains array = []

        var linuxFxVersion = '${toUpper(runtimeStack)}|${runtimeVersion}'
        var workerRuntime = toUpper(runtimeStack) == 'DOTNET'
          ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet')
          : toLower(runtimeStack)

        resource functionApp '{{FunctionAppArmType}}' = {
          name: name
          location: location
          kind: 'functionapp'
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: linuxFxVersion
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
              appSettings: [
                {
                  name: 'FUNCTIONS_WORKER_RUNTIME'
                  value: workerRuntime
                }
                {
                  name: 'FUNCTIONS_EXTENSION_VERSION'
                  value: '~4'
                }
              ]
            }
          }
        }

        resource hostNameBindings '{{HostNameBindingsArmType}}' = [for domain in customDomains: {
          parent: functionApp
          name: domain.domainName
          properties: {
            siteName: functionApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Function App')
        output id string = functionApp.id

        @description('The default host name of the Function App')
        output defaultHostName string = functionApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = functionApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = functionApp.properties.customDomainVerificationId
        """;

    private static readonly string FunctionAppContainerManagedIdentityModuleTemplate = $$"""
        import { RuntimeStack, WorkerRuntime } from './types.bicep'

        @description('Azure region for the Function App')
        param location string

        @description('Name of the Function App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Function App')
        param runtimeStack RuntimeStack = 'DOTNET'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Container'

        @description('Docker image name (e.g. myapp/functions)')
        param dockerImageName string

        @description('Docker image tag (e.g. latest, v1.2.3)')
        param dockerImageTag string = 'latest'

        @description('ACR login server (e.g. myregistry.azurecr.io)')
        param acrLoginServer string

        @description('Whether to use managed identity credentials for ACR')
        param acrUseManagedIdentityCreds bool = true

        @description('Client ID of the user-assigned managed identity for ACR pull')
        param acrUserManagedIdentityId string = ''

        @description('Custom domain bindings for this Function App')
        param customDomains array = []

        var dockerImage = '${acrLoginServer}/${dockerImageName}:${dockerImageTag}'
        var workerRuntime = toUpper(runtimeStack) == 'DOTNET'
          ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet')
          : toLower(runtimeStack)

        resource functionApp '{{FunctionAppArmType}}' = {
          name: name
          location: location
          kind: 'functionapp,linux,container'
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: 'DOCKER|${dockerImage}'
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
              acrUseManagedIdentityCreds: acrUseManagedIdentityCreds
              acrUserManagedIdentityID: !empty(acrUserManagedIdentityId) ? acrUserManagedIdentityId : null
              appSettings: [
                {
                  name: 'FUNCTIONS_WORKER_RUNTIME'
                  value: workerRuntime
                }
                {
                  name: 'FUNCTIONS_EXTENSION_VERSION'
                  value: '~4'
                }
              ]
            }
          }
        }

        resource hostNameBindings '{{HostNameBindingsArmType}}' = [for domain in customDomains: {
          parent: functionApp
          name: domain.domainName
          properties: {
            siteName: functionApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Function App')
        output id string = functionApp.id

        @description('The default host name of the Function App')
        output defaultHostName string = functionApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = functionApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = functionApp.properties.customDomainVerificationId
        """;

    private static readonly string FunctionAppContainerAdminCredentialsModuleTemplate = $$"""
        import { RuntimeStack, WorkerRuntime } from './types.bicep'

        @description('Azure region for the Function App')
        param location string

        @description('Name of the Function App')
        param name string

        @description('Resource ID of the App Service Plan')
        param appServicePlanId string

        @description('Runtime stack of the Function App')
        param runtimeStack RuntimeStack = 'DOTNET'

        @description('Runtime version (e.g. 8.0, 18)')
        param runtimeVersion string

        @description('Whether HTTPS only is enforced')
        param httpsOnly bool

        @description('Deployment mode')
        param deploymentMode string = 'Container'

        @description('Docker image name (e.g. myapp/functions)')
        param dockerImageName string

        @description('Docker image tag (e.g. latest, v1.2.3)')
        param dockerImageTag string = 'latest'

        @description('ACR login server (e.g. myregistry.azurecr.io)')
        param acrLoginServer string

        @secure()
        @description('Admin password for the Container Registry')
        param acrPassword string

        @description('Custom domain bindings for this Function App')
        param customDomains array = []

        var dockerImage = '${acrLoginServer}/${dockerImageName}:${dockerImageTag}'
        var workerRuntime = toUpper(runtimeStack) == 'DOTNET'
          ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet')
          : toLower(runtimeStack)
        var acrUsername = split(acrLoginServer, '.')[0]

        resource functionApp '{{FunctionAppArmType}}' = {
          name: name
          location: location
          kind: 'functionapp,linux,container'
          properties: {
            serverFarmId: appServicePlanId
            httpsOnly: httpsOnly
            siteConfig: {
              linuxFxVersion: 'DOCKER|${dockerImage}'
              ftpsState: 'Disabled'
              minTlsVersion: '1.2'
              acrUseManagedIdentityCreds: false
              appSettings: [
                {
                  name: 'FUNCTIONS_WORKER_RUNTIME'
                  value: workerRuntime
                }
                {
                  name: 'FUNCTIONS_EXTENSION_VERSION'
                  value: '~4'
                }
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

        resource hostNameBindings '{{HostNameBindingsArmType}}' = [for domain in customDomains: {
          parent: functionApp
          name: domain.domainName
          properties: {
            siteName: functionApp.name
            hostNameType: 'Verified'
            sslState: domain.bindingType == 'SniEnabled' ? 'SniEnabled' : 'Disabled'
          }
        }]

        @description('The resource ID of the Function App')
        output id string = functionApp.id

        @description('The default host name of the Function App')
        output defaultHostName string = functionApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = functionApp.identity.principalId

        @description('The custom domain verification ID')
        output customDomainVerificationId string = functionApp.properties.customDomainVerificationId
        """;
}
