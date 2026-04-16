using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class AppServicePlanTypeBicepGenerator
    : IResourceTypeBicepGenerator
{
    public string ResourceType
        => AzureResourceTypes.ArmTypes.AppServicePlan;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.AppServicePlan;

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "appServicePlan",
            ModuleFileName = "appServicePlan",
            ModuleFolderName = "AppServicePlan",
            ModuleBicepContent = AppServicePlanModuleTemplate,
            ModuleTypesBicepContent = AppServicePlanTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>
            {
                ["sku"] = resource.Properties.GetValueOrDefault("sku", "F1"),
                ["capacity"] = int.TryParse(resource.Properties.GetValueOrDefault("capacity", "1"), out var cap) ? cap : 1,
                ["osType"] = resource.Properties.GetValueOrDefault("osType", "Linux"),
            }
        };
    }

    private const string AppServicePlanTypesTemplate = """
        @export()
        @description('SKU name for the App Service Plan')
        type SkuName = 'F1' | 'D1' | 'B1' | 'B2' | 'B3' | 'S1' | 'S2' | 'S3' | 'P1v2' | 'P2v2' | 'P3v2' | 'P1v3' | 'P2v3' | 'P3v3' | 'I1' | 'I2' | 'I3' | 'I1v2' | 'I2v2' | 'I3v2'

        @export()
        @description('Operating system type for the App Service Plan')
        type OsType = 'Linux' | 'Windows'
        """;

    private static readonly string AppServicePlanModuleTemplate = $$"""
        import { SkuName, OsType } from './types.bicep'

        @description('Azure region for the App Service Plan')
        param location string

        @description('Name of the App Service Plan')
        param name string

        @description('SKU name of the App Service Plan')
        param sku SkuName = 'F1'

        @description('Number of instances allocated to the plan')
        param capacity int

        @description('Operating system type')
        param osType OsType = 'Linux'

        var isLinux = osType == 'Linux'
        var kind = isLinux ? 'linux' : 'app'

        resource asp 'Microsoft.Web/serverfarms@{{AzureResourceTypes.ApiVersions.AppServicePlan}}' = {
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
