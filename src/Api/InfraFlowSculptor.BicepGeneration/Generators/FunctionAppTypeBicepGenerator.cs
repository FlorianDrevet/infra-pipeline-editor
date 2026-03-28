using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>Generates Bicep modules for Azure Function App resources.</summary>
public sealed class FunctionAppTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => "Microsoft.Web/sites/functionapp";

    /// <inheritdoc />
    public string ResourceTypeName => "FunctionApp";

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var runtimeStack = resource.Properties.GetValueOrDefault("runtimeStack", "DOTNET");
        var runtimeVersion = resource.Properties.GetValueOrDefault("runtimeVersion", "8.0");
        var httpsOnly = resource.Properties.GetValueOrDefault("httpsOnly", "true") == "true";
        var functionsWorkerRuntime = resource.Properties.GetValueOrDefault("functionsWorkerRuntime", "");

        return new GeneratedTypeModule
        {
            ModuleName = "functionApp",
            ModuleFileName = "functionApp",
            ModuleFolderName = "FunctionApp",
            ModuleBicepContent = FunctionAppModuleTemplate,
            ModuleTypesBicepContent = FunctionAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["runtimeStack"] = runtimeStack,
                ["runtimeVersion"] = runtimeVersion,
                ["httpsOnly"] = httpsOnly,
                ["functionsWorkerRuntime"] = functionsWorkerRuntime,
            }
        };
    }

    private const string FunctionAppTypesTemplate = """
        @export()
        @description('Runtime stack for the Function App')
        type RuntimeStack = 'DOTNET' | 'NODE' | 'PYTHON' | 'JAVA' | 'POWERSHELL'

        @export()
        @description('Functions worker runtime identifier')
        type WorkerRuntime = 'dotnet' | 'dotnet-isolated' | 'node' | 'python' | 'java' | 'powershell'
        """;

    private const string FunctionAppModuleTemplate = """
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

        @description('Functions worker runtime override (empty uses runtimeStack)')
        param functionsWorkerRuntime string = ''

        var linuxFxVersion = '${toUpper(runtimeStack)}|${runtimeVersion}'
        var workerRuntime = !empty(functionsWorkerRuntime) ? functionsWorkerRuntime : toLower(runtimeStack)

        resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
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
}
