using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Container Registry (<c>Microsoft.ContainerRegistry/registries</c>).
/// </summary>
public sealed class ContainerRegistryTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "containerRegistry";
    private const string ModuleFolderName = "ContainerRegistry";
    private const string ModuleFileName = "containerRegistry";
    private const string SkuNameTypeName = "SkuName";
    private const string PublicNetworkAccessTypeName = "PublicNetworkAccess";
    private const string SkuParameterName = "sku";
    private const string AdminUserEnabledParameterName = "adminUserEnabled";
    private const string PublicNetworkAccessParameterName = "publicNetworkAccess";
    private const string ZoneRedundancyParameterName = "zoneRedundancy";
    private const string ResourceSymbol = "containerRegistry";
    private const string ContainerRegistryArmType = "Microsoft.ContainerRegistry/registries@2023-07-01";
    private const string DefaultSkuName = "Basic";
    private const string DefaultPublicNetworkAccess = "Enabled";
    private const string EnabledStateValue = "Enabled";
    private const string DisabledStateValue = "Disabled";
    private const string LoginServerOutputName = "loginServer";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string LoginServerExpression = ResourceSymbol + ".properties.loginServer";
    private const string SkuNameUnion = "'Basic' | 'Standard' | 'Premium'";
    private const string PublicNetworkAccessUnion = "'Enabled' | 'Disabled'";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.ContainerRegistry;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.ContainerRegistry;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SkuNameTypeName, PublicNetworkAccessTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Container Registry")
            .Param(NameParameterName, BicepType.String, "Name of the Container Registry")
            .Param(SkuParameterName, BicepType.Custom(SkuNameTypeName), "SKU of the Container Registry",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Param(AdminUserEnabledParameterName, BicepType.Bool, "Whether the admin user is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(PublicNetworkAccessParameterName, BicepType.Custom(PublicNetworkAccessTypeName), "Public network access setting",
                defaultValue: new BicepStringLiteral(DefaultPublicNetworkAccess))
            .Param(ZoneRedundancyParameterName, BicepType.Bool, "Whether zone redundancy is enabled (Premium only)",
                defaultValue: new BicepBoolLiteral(false))
            .Resource(ResourceSymbol, ContainerRegistryArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(SkuParameterName, sku => sku
                .Property(NamePropertyName, new BicepReference(SkuParameterName)))
            .Property(PropertiesPropertyName, props => props
                .Property(AdminUserEnabledParameterName, new BicepReference(AdminUserEnabledParameterName))
                .Property(PublicNetworkAccessParameterName, new BicepReference(PublicNetworkAccessParameterName))
                .Property(ZoneRedundancyParameterName, new BicepConditionalExpression(
                    new BicepReference(ZoneRedundancyParameterName),
                    new BicepStringLiteral(EnabledStateValue),
                    new BicepStringLiteral(DisabledStateValue))))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Container Registry")
            .Output(LoginServerOutputName, BicepType.String, new BicepRawExpression(LoginServerExpression),
                description: "The login server of the Container Registry")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU for the Container Registry")
            .ExportedType(PublicNetworkAccessTypeName,
                new BicepRawExpression(PublicNetworkAccessUnion),
                description: "Public network access setting for the Container Registry")
            .Build();
    }

    /// <inheritdoc />
    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleFileName,
            ModuleFolderName = ModuleFolderName,
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
