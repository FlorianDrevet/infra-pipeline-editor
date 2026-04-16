using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep modules for Azure Function App resources with Code or Container deployment modes.</summary>
public sealed class FunctionAppTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.FunctionApp;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.FunctionApp;

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var deploymentMode = resource.Properties.GetValueOrDefault("deploymentMode", "Code");
        var isContainer = string.Equals(deploymentMode, "Container", StringComparison.OrdinalIgnoreCase);

        var runtimeStack = resource.Properties.GetValueOrDefault("runtimeStack", "DOTNET");
        var runtimeVersion = resource.Properties.GetValueOrDefault("runtimeVersion", "8.0");
        var httpsOnly = resource.Properties.GetValueOrDefault("httpsOnly", "true") == "true";
        var dockerImageName = resource.Properties.GetValueOrDefault("dockerImageName", "");

        var parameters = new Dictionary<string, object>
        {
            ["runtimeStack"] = runtimeStack,
            ["runtimeVersion"] = runtimeVersion,
            ["httpsOnly"] = httpsOnly,
            ["deploymentMode"] = deploymentMode,
        };

        if (isContainer)
        {
            parameters["dockerImageName"] = dockerImageName;
            parameters["dockerImageTag"] = "latest";
            parameters["acrLoginServer"] = "";
            parameters["acrUseManagedIdentityCreds"] = true;
            parameters["acrUserManagedIdentityId"] = "";
        }

        return new GeneratedTypeModule
        {
            ModuleName = "functionApp",
            ModuleFileName = "functionApp",
            ModuleFolderName = "FunctionApp",
            ModuleBicepContent = isContainer ? FunctionAppContainerModuleTemplate : FunctionAppCodeModuleTemplate,
            ModuleTypesBicepContent = FunctionAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = parameters
        };
    }

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

        var linuxFxVersion = '${toUpper(runtimeStack)}|${runtimeVersion}'
        var workerRuntime = toUpper(runtimeStack) == 'DOTNET'
          ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet')
          : toLower(runtimeStack)

        resource functionApp 'Microsoft.Web/sites@{{AzureResourceTypes.ApiVersions.FunctionApp}}' = {
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

        @description('The resource ID of the Function App')
        output id string = functionApp.id

        @description('The default host name of the Function App')
        output defaultHostName string = functionApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = functionApp.identity.principalId
        """;

    private static readonly string FunctionAppContainerModuleTemplate = $$"""
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

        var dockerImage = '${acrLoginServer}/${dockerImageName}:${dockerImageTag}'
        var workerRuntime = toUpper(runtimeStack) == 'DOTNET'
          ? (contains(runtimeVersion, 'isolated') ? 'dotnet-isolated' : 'dotnet')
          : toLower(runtimeStack)

        resource functionApp 'Microsoft.Web/sites@{{AzureResourceTypes.ApiVersions.FunctionApp}}' = {
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

        @description('The resource ID of the Function App')
        output id string = functionApp.id

        @description('The default host name of the Function App')
        output defaultHostName string = functionApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = functionApp.identity.principalId
        """;
}
