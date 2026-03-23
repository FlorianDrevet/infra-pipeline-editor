using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class AppServicePlanTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => "Microsoft.Web/serverfarms";

    /// <inheritdoc />
    public string ResourceTypeName => "AppServicePlan";

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "appServicePlan",
            ModuleFileName = "appServicePlan.bicep",
            ModuleBicepContent = AppServicePlanModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Properties.GetValueOrDefault("sku", "F1"),
                ["capacity"] = int.TryParse(resource.Properties.GetValueOrDefault("capacity", "1"), out var cap) ? cap : 1,
                ["osType"] = resource.Properties.GetValueOrDefault("osType", "Linux"),
            }
        };
    }

    private const string AppServicePlanModuleTemplate = """
        param location string
        param name string
        param sku string
        param capacity int
        param osType string

        var isLinux = osType == 'Linux'
        var kind = isLinux ? 'linux' : 'app'

        resource asp 'Microsoft.Web/serverfarms@2023-12-01' = {
          name: name
          location: location
          kind: kind
          sku: {
            name: sku
            capacity: capacity
          }
          properties: {
            reserved: isLinux
          }
        }

        output id string = asp.id
        """;
}
