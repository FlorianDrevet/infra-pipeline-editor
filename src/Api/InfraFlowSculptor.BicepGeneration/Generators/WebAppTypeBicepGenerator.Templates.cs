namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed partial class WebAppTypeBicepGenerator
{
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
