using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Network Security Group (<c>Microsoft.Network/networkSecurityGroups@2023-11-01</c>).
/// </summary>
public sealed class NetworkSecurityGroupTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "networkSecurityGroup";
    private const string ModuleFolderName = "NetworkSecurityGroup";
    private const string ModuleFileName = "networkSecurityGroup";
    private const string SecurityRulesParameterName = "securityRules";
    private const string TagsParameterName = "tags";
    private const string ResourceSymbol = "nsg";
    private const string NetworkSecurityGroupArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.NetworkSecurityGroupArmType;
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string ResourceNameExpression = ResourceSymbol + ".name";
    private const string NameOutputName = "nameOutput";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.NetworkSecurityGroupType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.NetworkSecurityGroup;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Network Security Group")
            .Param(NameParameterName, BicepType.String, "Name of the Network Security Group")
            .Param(SecurityRulesParameterName, BicepType.Array, "Security rules for the NSG",
                defaultValue: new BicepArrayExpression([]))
            .Param(TagsParameterName, BicepType.Object, "Resource tags",
                defaultValue: BicepObjectExpression.Empty)
            .Resource(ResourceSymbol, NetworkSecurityGroupArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property("tags", new BicepReference(TagsParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(SecurityRulesParameterName, new BicepReference(SecurityRulesParameterName)))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Network Security Group")
            .Output(NameOutputName, BicepType.String, new BicepRawExpression(ResourceNameExpression),
                description: "The name of the Network Security Group")
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
            ModuleBicepContent = NsgModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private static readonly string NsgModuleTemplate = $$"""
        @description('Azure region for the Network Security Group')
        param location string

        @description('Name of the Network Security Group')
        param name string

        @description('Security rules for the NSG')
        param securityRules array = []

        @description('Resource tags')
        param tags object = {}

        resource nsg '{{NetworkSecurityGroupArmType}}' = {
          name: name
          location: location
          tags: tags
          properties: {
            securityRules: securityRules
          }
        }

        @description('The resource ID of the Network Security Group')
        output id string = nsg.id

        @description('The name of the Network Security Group')
        output nameOutput string = nsg.name
        """;
}
