using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;
using static InfraFlowSculptor.BicepGeneration.Generators.Constants.BicepGeneratorSharedConstants;

namespace InfraFlowSculptor.BicepGeneration.Generators;

/// <summary>
/// Generates a Bicep companion module for Azure Private Endpoint (<c>Microsoft.Network/privateEndpoints@2023-11-01</c>).
/// This generator is not registered as a primary resource type generator; it produces a reusable
/// Private Endpoint module with integrated Private DNS Zone Group.
/// </summary>
public sealed class PrivateEndpointTypeBicepGenerator
    : IResourceTypeBicepSpecGenerator
{
    private const string ModuleName = "privateEndpoint";
    private const string ModuleFolderName = "Common";
    private const string ModuleFileName = "privateEndpoint";
    private const string PrivateLinkServiceIdParameterName = "privateLinkServiceId";
    private const string GroupIdsParameterName = "groupIds";
    private const string SubnetIdParameterName = "subnetId";
    private const string PrivateDnsZoneIdParameterName = "privateDnsZoneId";
    private const string CustomNetworkInterfaceNameParameterName = "customNetworkInterfaceName";
    private const string TagsParameterName = "tags";
    private const string ResourceSymbol = "privateEndpoint";
    private const string DnsZoneGroupSymbol = "dnsZoneGroup";
    private const string PrivateEndpointArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.PrivateEndpointArmType;
    private const string DnsZoneGroupArmType = InfraFlowSculptor.BicepGeneration.Constants.BicepArmTypeCatalog.PrivateEndpointPrivateDnsZoneGroupsArmType;
    private const string PrivateLinkServiceConnectionsPropertyName = "privateLinkServiceConnections";
    private const string SubnetPropertyName = "subnet";
    private const string CustomNetworkInterfaceNamePropertyName = "customNetworkInterfaceName";
    private const string PrivateDnsZoneConfigsPropertyName = "privateDnsZoneConfigs";
    private const string PrivateDnsZoneIdPropertyName = "privateDnsZoneId";
    private const string ResourceIdExpression = ResourceSymbol + ".id";

    /// <inheritdoc />
    public string ResourceType
        => AzureResourceTypes.ArmTypes.PrivateEndpointType;

    /// <inheritdoc />
    public string ResourceTypeName => "PrivateEndpoint";

    /// <inheritdoc />
    public BicepModuleSpec GenerateSpec(ResourceDefinition resource)
    {
        return new BicepModuleBuilder()
            .Module(ModuleName, ModuleFolderName, ResourceTypeName)
            .Param(LocationParameterName, BicepType.String, "Azure region for the Private Endpoint")
            .Param(NameParameterName, BicepType.String, "Name of the Private Endpoint")
            .Param(PrivateLinkServiceIdParameterName, BicepType.String, "Resource ID of the service to connect to")
            .Param(GroupIdsParameterName, BicepType.Array, "Group IDs for the private link service connection (e.g. ['vault'])")
            .Param(SubnetIdParameterName, BicepType.String, "Resource ID of the subnet for the Private Endpoint")
            .Param(PrivateDnsZoneIdParameterName, BicepType.String, "Resource ID of the Private DNS Zone for DNS integration",
                defaultValue: new BicepStringLiteral(""))
            .Param(CustomNetworkInterfaceNameParameterName, BicepType.String, "Custom name for the network interface",
                defaultValue: new BicepStringLiteral(""))
            .Param(TagsParameterName, BicepType.Object, "Resource tags",
                defaultValue: BicepObjectExpression.Empty)
            .Resource(ResourceSymbol, PrivateEndpointArmType)
            .Property(NamePropertyName, new BicepReference(NameParameterName))
            .Property(LocationPropertyName, new BicepReference(LocationParameterName))
            .Property("tags", new BicepReference(TagsParameterName))
            .Property(PropertiesPropertyName, props => props
                .Property(PrivateLinkServiceConnectionsPropertyName, new BicepArrayExpression(
                [
                    new BicepObjectExpression(
                    [
                        new BicepPropertyAssignment(NamePropertyName, new BicepReference(NameParameterName)),
                        new BicepPropertyAssignment(PropertiesPropertyName, new BicepObjectExpression(
                        [
                            new BicepPropertyAssignment(PrivateLinkServiceIdParameterName, new BicepReference(PrivateLinkServiceIdParameterName)),
                            new BicepPropertyAssignment(GroupIdsParameterName, new BicepReference(GroupIdsParameterName)),
                        ])),
                    ]),
                ]))
                .Property(SubnetPropertyName, subnet => subnet
                    .Property(IdOutputName, new BicepReference(SubnetIdParameterName)))
                .Property(CustomNetworkInterfaceNamePropertyName,
                    new BicepConditionalExpression(
                        new BicepRawExpression($"!empty({CustomNetworkInterfaceNameParameterName})"),
                        new BicepReference(CustomNetworkInterfaceNameParameterName),
                        new BicepRawExpression("null"))))
            .AdditionalResource(DnsZoneGroupSymbol, DnsZoneGroupArmType,
                parentSymbol: ResourceSymbol,
                condition: new BicepRawExpression($"!empty({PrivateDnsZoneIdParameterName})"),
                bodyBuilder: body => body
                    .Property(NamePropertyName, new BicepStringLiteral("default"))
                    .Property(PropertiesPropertyName, props => props
                        .Property(PrivateDnsZoneConfigsPropertyName, new BicepArrayExpression(
                        [
                            new BicepObjectExpression(
                            [
                                new BicepPropertyAssignment(NamePropertyName, new BicepStringLiteral("config")),
                                new BicepPropertyAssignment(PropertiesPropertyName, new BicepObjectExpression(
                                [
                                    new BicepPropertyAssignment(PrivateDnsZoneIdPropertyName, new BicepReference(PrivateDnsZoneIdParameterName)),
                                ])),
                            ]),
                        ]))))
            .Output(IdOutputName, BicepType.String, new BicepRawExpression(ResourceIdExpression),
                description: "The resource ID of the Private Endpoint")
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
            ModuleBicepContent = PrivateEndpointModuleTemplate,
            ResourceTypeName = ResourceTypeName,
            Parameters = new Dictionary<string, object>()
        };
    }

    private static readonly string PrivateEndpointModuleTemplate = $$"""
        @description('Azure region for the Private Endpoint')
        param location string

        @description('Name of the Private Endpoint')
        param name string

        @description('Resource ID of the service to connect to')
        param privateLinkServiceId string

        @description('Group IDs for the private link service connection (e.g. [\'vault\'])')
        param groupIds array

        @description('Resource ID of the subnet for the Private Endpoint')
        param subnetId string

        @description('Resource ID of the Private DNS Zone for DNS integration')
        param privateDnsZoneId string = ''

        @description('Custom name for the network interface')
        param customNetworkInterfaceName string = ''

        @description('Resource tags')
        param tags object = {}

        resource privateEndpoint '{{PrivateEndpointArmType}}' = {
          name: name
          location: location
          tags: tags
          properties: {
            privateLinkServiceConnections: [
              {
                name: name
                properties: {
                  privateLinkServiceId: privateLinkServiceId
                  groupIds: groupIds
                }
              }
            ]
            subnet: {
              id: subnetId
            }
            customNetworkInterfaceName: !empty(customNetworkInterfaceName) ? customNetworkInterfaceName : null
          }
        }

        resource dnsZoneGroup '{{DnsZoneGroupArmType}}' = if (!empty(privateDnsZoneId)) {
          parent: privateEndpoint
          name: 'default'
          properties: {
            privateDnsZoneConfigs: [
              {
                name: 'config'
                properties: {
                  privateDnsZoneId: privateDnsZoneId
                }
              }
            ]
          }
        }

        @description('The resource ID of the Private Endpoint')
        output id string = privateEndpoint.id
        """;
}
