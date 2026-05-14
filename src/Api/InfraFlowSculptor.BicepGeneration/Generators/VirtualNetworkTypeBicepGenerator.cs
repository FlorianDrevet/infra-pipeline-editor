using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Virtual Network (<c>Microsoft.Network/virtualNetworks@2023-11-01</c>).
/// </summary>
public sealed class VirtualNetworkTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "virtualNetwork";
    private const string ModuleFolderName = "VirtualNetwork";
    private const string ModuleFileName = "virtualNetwork";
    private const string AddressPrefixesParameterName = "addressPrefixes";
    private const string EnableDdosProtectionParameterName = "enableDdosProtection";
    private const string SubnetsParameterName = "subnets";
    private const string TagsParameterName = "tags";
    private const string SubnetConfigTypeName = "SubnetConfig";
    private const string SubnetConfigTypeBody = """
        {
          name: string
          addressPrefix: string
          delegation: string?
          serviceEndpoints: string[]?
          privateEndpointNetworkPolicies: string?
          nsgId: string?
        }
        """;
    private const string ResourceSymbol = "virtualNetwork";
    private const string VirtualNetworkArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.VirtualNetworkArmType;
    private const string AddressSpacePropertyName = "addressSpace";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string ResourceNameExpression = ResourceSymbol + ".name";
    private const string NameOutputName = "nameOutput";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.VirtualNetworkType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.VirtualNetwork;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Import(TypesImportPath, SubnetConfigTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Virtual Network")
            .Param(NameParameterName, BicepType.String, "Name of the Virtual Network")
            .Param(AddressPrefixesParameterName, BicepType.Array, "Address space prefixes (e.g. ['10.0.0.0/16'])",
                defaultValue: new BicepArrayExpression([new BicepStringLiteral("10.0.0.0/16")]))
            .Param(EnableDdosProtectionParameterName, BicepType.Bool, "Whether DDoS protection is enabled",
                defaultValue: new BicepBoolLiteral(false))
            .Param(SubnetsParameterName, BicepType.Custom(SubnetConfigTypeName + "[]"), "Subnet configurations",
                defaultValue: new BicepArrayExpression([]))
            .Param(TagsParameterName, BicepType.Object, "Resource tags",
                defaultValue: BicepObjectExpression.Empty)
            .Resource(ResourceSymbol, VirtualNetworkArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property("tags", new BicepReference(TagsParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(AddressSpacePropertyName, addressSpace => addressSpace
                    .Property(AddressPrefixesParameterName, new BicepReference(AddressPrefixesParameterName)))
                .Property(EnableDdosProtectionParameterName, new BicepReference(EnableDdosProtectionParameterName))
                .Property(SubnetsParameterName, new BicepReference(SubnetsParameterName)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Virtual Network")
            .Output(NameOutputName, BicepType.String, new BicepRawExpression(ResourceNameExpression),
                description: "The name of the Virtual Network")
            .ExportedType(SubnetConfigTypeName,
                new BicepRawExpression(SubnetConfigTypeBody),
                description: "Subnet configuration for a Virtual Network")
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
            ModuleBicepContent = VirtualNetworkModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private static readonly string VirtualNetworkModuleTemplate = $$"""
        import { SubnetConfig } from './types.bicep'

        @description('Azure region for the Virtual Network')
        param location string

        @description('Name of the Virtual Network')
        param name string

        @description('Address space prefixes (e.g. [\'10.0.0.0/16\'])')
        param addressPrefixes array = ['10.0.0.0/16']

        @description('Whether DDoS protection is enabled')
        param enableDdosProtection bool = false

        @description('Subnet configurations')
        param subnets SubnetConfig[] = []

        @description('Resource tags')
        param tags object = {}

        resource virtualNetwork '{{VirtualNetworkArmType}}' = {
          name: name
          location: location
          tags: tags
          properties: {
            addressSpace: {
              addressPrefixes: addressPrefixes
            }
            enableDdosProtection: enableDdosProtection
            subnets: subnets
          }
        }

        @description('The resource ID of the Virtual Network')
        output id string = virtualNetwork.id

        @description('The name of the Virtual Network')
        output nameOutput string = virtualNetwork.name
        """;
}
