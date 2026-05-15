using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Front Door / CDN Profile (<c>Microsoft.Cdn/profiles@2024-02-01</c>).
/// </summary>
public sealed class FrontDoorTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "frontDoor";
    private const string ModuleFolderName = "FrontDoor";
    private const string ModuleFileName = "frontDoor";
    private const string SkuParameterName = "skuName";
    private const string TagsParameterName = "tags";
    private const string ResourceSymbol = "frontDoor";
    private const string FrontDoorArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.FrontDoorArmType;
    private const string SkuNameTypeName = "SkuName";
    private const string DefaultSkuName = "Standard_AzureFrontDoor";
    private const string OriginResponseTimeoutSecondsPropertyName = "originResponseTimeoutSeconds";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string ResourceNameExpression = ResourceSymbol + ".name";
    private const string FrontDoorIdExpression = ResourceSymbol + ".properties.frontDoorId";
    private const string NameOutputName = "nameOutput";
    private const string FrontDoorIdOutputName = "frontDoorId";
    private const string SkuNameUnion = "'Standard_AzureFrontDoor' | 'Premium_AzureFrontDoor'";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.FrontDoorType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.FrontDoor;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SkuNameTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Front Door profile")
            .Param(NameParameterName, BicepType.String, "Name of the Front Door profile")
            .Param(SkuParameterName, BicepType.Custom(SkuNameTypeName), "SKU name for the Front Door profile",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Param(TagsParameterName, BicepType.Object, "Resource tags",
                defaultValue: BicepObjectExpression.Empty)
            .Resource(ResourceSymbol, FrontDoorArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property("tags", new BicepReference(TagsParameterName))
            .Property(SkuParameterName, sku => sku
                .Property(NamePropertyName, new BicepReference(SkuParameterName)))
            .Property(PropertiesPropertyName, props => props
                .Property(OriginResponseTimeoutSecondsPropertyName, new BicepIntLiteral(60)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Front Door profile")
            .Output(NameOutputName, BicepType.String, new BicepRawExpression(ResourceNameExpression),
                description: "The name of the Front Door profile")
            .Output(FrontDoorIdOutputName, BicepType.String, new BicepRawExpression(FrontDoorIdExpression),
                description: "The Front Door ID used for header validation")
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU for the Front Door profile")
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
            ModuleBicepContent = FrontDoorModuleTemplate,
            ModuleTypesBicepContent = FrontDoorTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private const string FrontDoorTypesTemplate = """
        @export()
        @description('SKU for the Front Door profile')
        type SkuName = 'Standard_AzureFrontDoor' | 'Premium_AzureFrontDoor'
        """;

    private static readonly string FrontDoorModuleTemplate = $$"""
        import { SkuName } from './types.bicep'

        @description('Azure region for the Front Door profile')
        param location string

        @description('Name of the Front Door profile')
        param name string

        @description('SKU name for the Front Door profile')
        param skuName SkuName = 'Standard_AzureFrontDoor'

        @description('Resource tags')
        param tags object = {}

        resource frontDoor '{{FrontDoorArmType}}' = {
          name: name
          location: location
          tags: tags
          sku: {
            name: skuName
          }
          properties: {
            originResponseTimeoutSeconds: 60
          }
        }

        @description('The resource ID of the Front Door profile')
        output id string = frontDoor.id

        @description('The name of the Front Door profile')
        output nameOutput string = frontDoor.name

        @description('The Front Door ID used for header validation')
        output frontDoorId string = frontDoor.properties.frontDoorId
        """;
}
