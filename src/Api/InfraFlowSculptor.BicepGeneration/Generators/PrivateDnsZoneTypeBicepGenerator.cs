using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep module for Azure Private DNS Zone (<c>Microsoft.Network/privateDnsZones@2024-06-01</c>).
/// Location is always <c>global</c> for Private DNS Zones.
/// </summary>
public sealed class PrivateDnsZoneTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "privateDnsZone";
    private const string ModuleFolderName = "PrivateDnsZone";
    private const string ModuleFileName = "privateDnsZone";
    private const string VirtualNetworkLinksParameterName = "virtualNetworkLinks";
    private const string TagsParameterName = "tags";
    private const string ResourceSymbol = "privateDnsZone";
    private const string LinkSymbol = "vnetLink";
    private const string PrivateDnsZoneArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.PrivateDnsZoneArmType;
    private const string VnetLinkArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.PrivateDnsZoneVirtualNetworkLinksArmType;
    private const string GlobalLocation = "global";
    private const string ResourceIdExpression = ResourceSymbol + ".id";
    private const string ResourceNameExpression = ResourceSymbol + ".name";
    private const string NameOutputName = "nameOutput";
    private const string RegistrationEnabledPropertyName = "registrationEnabled";
    private const string VirtualNetworkPropertyName = "virtualNetwork";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.PrivateDnsZoneType;

    /// <inheritdoc />
    public string ResourceTypeName => AzureResourceTypes.PrivateDnsZone;

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Param(NameParameterName, BicepType.String, "Private DNS zone name (e.g. privatelink.vaultcore.azure.net)")
            .Param(VirtualNetworkLinksParameterName, BicepType.Array, "Virtual network links with vnetId and enableAutoRegistration",
                defaultValue: new BicepArrayExpression([]))
            .Param(TagsParameterName, BicepType.Object, "Resource tags",
                defaultValue: BicepObjectExpression.Empty)
            .Resource(ResourceSymbol, PrivateDnsZoneArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepStringLiteral(GlobalLocation))
            .Property("tags", new BicepReference(TagsParameterName))
            .AdditionalResource(LinkSymbol, VnetLinkArmType,
                parentSymbol: ResourceSymbol,
                forLoop: new BicepForLoop("link", new BicepReference(VirtualNetworkLinksParameterName)),
                bodyBuilder: body => body
                    .Property(NamePropertyName, new BicepRawExpression("link.name"))
                    .Property(LocationPropertyName, new BicepStringLiteral(GlobalLocation))
                    .Property(PropertiesPropertyName, props => props
                        .Property(VirtualNetworkPropertyName, vnet => vnet
                            .Property(IdOutputName, new BicepRawExpression("link.vnetId")))
                        .Property(RegistrationEnabledPropertyName, new BicepRawExpression("link.enableAutoRegistration"))))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Private DNS Zone")
            .Output(NameOutputName, BicepType.String, new BicepRawExpression(ResourceNameExpression),
                description: "The name of the Private DNS Zone")
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
            ModuleBicepContent = PrivateDnsZoneModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private static readonly string PrivateDnsZoneModuleTemplate = $$"""
        @description('Private DNS zone name (e.g. privatelink.vaultcore.azure.net)')
        param name string

        @description('Virtual network links with vnetId and enableAutoRegistration')
        param virtualNetworkLinks array = []

        @description('Resource tags')
        param tags object = {}

        resource privateDnsZone '{{PrivateDnsZoneArmType}}' = {
          name: name
          location: 'global'
          tags: tags
        }

        resource vnetLink '{{VnetLinkArmType}}' = [for link in virtualNetworkLinks: {
          parent: privateDnsZone
          name: link.name
          location: 'global'
          properties: {
            virtualNetwork: {
              id: link.vnetId
            }
            registrationEnabled: link.enableAutoRegistration
          }
        }]

        @description('The resource ID of the Private DNS Zone')
        output id string = privateDnsZone.id

        @description('The name of the Private DNS Zone')
        output nameOutput string = privateDnsZone.name
        """;
}
