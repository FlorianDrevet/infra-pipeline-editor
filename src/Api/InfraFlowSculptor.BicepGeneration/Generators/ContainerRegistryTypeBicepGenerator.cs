using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container Registry (<c>Microsoft.ContainerRegistry/registries</c>).
/// </summary>
public sealed class ContainerRegistryTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerRegistry;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ContainerRegistry;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module("containerRegistry", "ContainerRegistry", ResourceTypeName)
            .Import("./types.bicep", "SkuName", "PublicNetworkAccess")
            .Param("location", BicepType.String, "Azure region for the Container Registry")
            .Param("name", BicepType.String, "Name of the Container Registry")
            .Param("sku", BicepType.Custom("SkuName"), "SKU of the Container Registry",
                defaultValue: new BicepStringLiteral("Basic"))
            .Param("adminUserEnabled", BicepType.Bool, "Whether the admin user is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param("publicNetworkAccess", BicepType.Custom("PublicNetworkAccess"), "Public network access setting",
                defaultValue: new BicepStringLiteral("Enabled"))
            .Param("zoneRedundancy", BicepType.Bool, "Whether zone redundancy is enabled (Premium only)",
                defaultValue: new BicepBoolLiteral(false))
            .Resource("containerRegistry", "Microsoft.ContainerRegistry/registries@2023-07-01")
            .Property("name", new BicepReference("name"))
            .Property("location", new BicepReference("location"))
            .Property("sku", sku => sku
                .Property("name", new BicepReference("sku")))
            .Property("properties", props => props
                .Property("adminUserEnabled", new BicepReference("adminUserEnabled"))
                .Property("publicNetworkAccess", new BicepReference("publicNetworkAccess"))
                .Property("zoneRedundancy", new BicepConditionalExpression(
                    new BicepReference("zoneRedundancy"),
                    new BicepStringLiteral("Enabled"),
                    new BicepStringLiteral("Disabled"))))
            .Output("id", BicepType.String, new BicepRawExpression("containerRegistry.id"),
                description: "The resource ID of the Container Registry")
            .Output("loginServer", BicepType.String, new BicepRawExpression("containerRegistry.properties.loginServer"),
                description: "The login server of the Container Registry")
            .ExportedType("SkuName",
                new BicepRawExpression("'Basic' | 'Standard' | 'Premium'"),
                description: "SKU for the Container Registry")
            .ExportedType("PublicNetworkAccess",
                new BicepRawExpression("'Enabled' | 'Disabled'"),
                description: "Public network access setting for the Container Registry")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = "containerRegistry",
            ModuleFileName = "containerRegistry",
            ModuleFolderName = "ContainerRegistry",
            ModuleBicepContent = ContainerRegistryModuleTemplate,
            ModuleTypesBicepContent = ContainerRegistryTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string ContainerRegistryTypesTemplate = """
        @export()
        @description('SKU for the Container Registry')
        type SkuName = 'Basic' | 'Standard' | 'Premium'

        @export()
        @description('Public network access setting for the Container Registry')
        type PublicNetworkAccess = 'Enabled' | 'Disabled'
        """;

    private const string ContainerRegistryModuleTemplate = """
        import { SkuName, PublicNetworkAccess } from './types.bicep'

        @description('Azure region for the Container Registry')
        param location string

        @description('Name of the Container Registry')
        param name string

        @description('SKU of the Container Registry')
        param sku SkuName = 'Basic'

        @description('Whether the admin user is enabled')
        param adminUserEnabled bool = false

        @description('Public network access setting')
        param publicNetworkAccess PublicNetworkAccess = 'Enabled'

        @description('Whether zone redundancy is enabled (Premium only)')
        param zoneRedundancy bool = false

        resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
          name: name
          location: location
          sku: {
            name: sku
          }
          properties: {
            adminUserEnabled: adminUserEnabled
            publicNetworkAccess: publicNetworkAccess
            zoneRedundancy: zoneRedundancy ? 'Enabled' : 'Disabled'
          }
        }

        @description('The resource ID of the Container Registry')
        output id string = containerRegistry.id

        @description('The login server of the Container Registry')
        output loginServer string = containerRegistry.properties.loginServer
        """;
}
