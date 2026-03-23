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
            ModuleFileName = "webApp.bicep",
            ModuleBicepContent = WebAppModuleTemplate,
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

    private const string WebAppModuleTemplate = """
        param location string
        param name string
        param appServicePlanId string
        param runtimeStack string
        param runtimeVersion string
        param alwaysOn bool
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
        """;
}
