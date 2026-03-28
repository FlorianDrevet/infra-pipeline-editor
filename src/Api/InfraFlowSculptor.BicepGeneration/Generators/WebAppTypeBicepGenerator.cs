using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class WebAppTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.Web/sites";

    /// <inheritdoc />
    public string ResourceTypeName => "WebApp";

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        var runtimeStack = resource.Properties.GetValueOrDefault("runtimeStack", "DOTNETCORE");
        var runtimeVersion = resource.Properties.GetValueOrDefault("runtimeVersion", "8.0");
        var alwaysOn = resource.Properties.GetValueOrDefault("alwaysOn", "true") == "true";
        var httpsOnly = resource.Properties.GetValueOrDefault("httpsOnly", "true") == "true";

        return new GeneratedTypeModule
        {
            ModuleName = "webApp",
            ModuleFileName = "webApp",
            ModuleFolderName = "WebApp",
            ModuleBicepContent = WebAppModuleTemplate,
            ModuleTypesBicepContent = WebAppTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["runtimeStack"] = runtimeStack,
                ["runtimeVersion"] = runtimeVersion,
                ["alwaysOn"] = alwaysOn,
                ["httpsOnly"] = httpsOnly,
            }
        };
    }

    private const string WebAppTypesTemplate = """
        @export()
        @description('Runtime stack for the Web App')
        type RuntimeStack = 'DOTNETCORE' | 'NODE' | 'PYTHON' | 'JAVA' | 'PHP'
        """;

    private const string WebAppModuleTemplate = """
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

        @description('The resource ID of the Web App')
        output id string = webApp.id

        @description('The default host name of the Web App')
        output defaultHostName string = webApp.properties.defaultHostName

        @description('The principal ID of the system-assigned managed identity')
        output principalId string = webApp.identity.principalId
        """;
}
