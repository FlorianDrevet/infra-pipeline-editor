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
            ModuleFileName = "functionApp.bicep",
            ModuleBicepContent = FunctionAppModuleTemplate,
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

    private const string FunctionAppModuleTemplate = """
        param location string
        param name string
        param appServicePlanId string
        param runtimeStack string
        param runtimeVersion string
        param httpsOnly bool
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
        """;
}
