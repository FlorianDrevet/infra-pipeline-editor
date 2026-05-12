using InfraFlowSculptor.BicepGeneration.Generators.ParameterModels;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

public sealed class AppServicePlanTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "appServicePlan";
    private const string ModuleFolderName = "AppServicePlan";
    private const string SkuNameTypeName = "SkuName";
    private const string OsTypeTypeName = "OsType";
    private const string SkuParameterName = "sku";
    private const string CapacityParameterName = "capacity";
    private const string OsTypeParameterName = "osType";
    private const string IsLinuxVariableName = "isLinux";
    private const string KindVariableName = "kind";
    private const string ResourceSymbol = "asp";
    private const string AppServicePlanArmType = "Microsoft.Web/serverfarms@2023-12-01";
    private const string DefaultSkuName = "F1";
    private const string DefaultOsType = "Linux";
    private const string LinuxKind = "linux";
    private const string AppKind = "app";
    private const string IsLinuxExpression = "osType == 'Linux'";
    private const string SkuNameUnion = "'F1' | 'D1' | 'B1' | 'B2' | 'B3' | 'S1' | 'S2' | 'S3' | 'P1v2' | 'P2v2' | 'P3v2' | 'P1v3' | 'P2v3' | 'P3v3' | 'I1' | 'I2' | 'I3' | 'I1v2' | 'I2v2' | 'I3v2'";
    private const string OsTypeUnion = "'Linux' | 'Windows'";
    private const string ReservedPropertyName = "reserved";
    private const string ResourceIdExpression = ResourceSymbol + ".id";

    public string ResourceType
        => AzureResourceTypes.ArmTypes.AppServicePlanType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.AppServicePlan;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SkuNameTypeName, OsTypeTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the App Service Plan")
            .Param(NameParameterName, BicepType.String, "Name of the App Service Plan")
            .Param(SkuParameterName, BicepType.Custom(SkuNameTypeName), "SKU name of the App Service Plan",
                defaultValue: new BicepStringLiteral(DefaultSkuName))
            .Param(CapacityParameterName, BicepType.Int, "Number of instances allocated to the plan")
            .Param(OsTypeParameterName, BicepType.Custom(OsTypeTypeName), "Operating system type",
                defaultValue: new BicepStringLiteral(DefaultOsType))
            .Var(IsLinuxVariableName, new BicepRawExpression(IsLinuxExpression))
            .Var(KindVariableName, new BicepConditionalExpression(
                new BicepReference(IsLinuxVariableName),
                new BicepStringLiteral(LinuxKind),
                new BicepStringLiteral(AppKind)))
            .Resource(ResourceSymbol, AppServicePlanArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property(KindPropertyName, new BicepReference(KindVariableName))
            .Property(SkuParameterName, sku => sku
                .Property(NamePropertyName, new BicepReference(SkuParameterName))
                .Property(CapacityParameterName, new BicepReference(CapacityParameterName)))
            .Property(PropertiesPropertyName, props => props
                .Property(ReservedPropertyName, new BicepReference(IsLinuxVariableName)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression))
            .ExportedType(SkuNameTypeName,
                new BicepRawExpression(SkuNameUnion),
                description: "SKU name for the App Service Plan")
            .ExportedType(OsTypeTypeName,
                new BicepRawExpression(OsTypeUnion),
                description: "Operating system type for the App Service Plan")
            .Build();
    }

    public GeneratedTypeModule Generate(ResourceDefinition resource)
    {
        return new GeneratedTypeModule
        {
            ModuleName = ModuleName,
            ModuleFileName = ModuleName,
            ModuleFolderName = ModuleFolderName,
            ModuleBicepContent = AppServicePlanModuleTemplate,
            ModuleTypesBicepContent = AppServicePlanTypesTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = BicepParameterModelConverter.ToDictionary(new AppServicePlanParameters
            {
                Sku = resource.Properties.GetValueOrDefault(SkuParameterName, DefaultSkuName),
                Capacity = int.TryParse(resource.Properties.GetValueOrDefault(CapacityParameterName, "1"), out var cap) ? cap : 1,
                OsType = resource.Properties.GetValueOrDefault(OsTypeParameterName, DefaultOsType),
            })
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

    private const string AppServicePlanModuleTemplate = """
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
