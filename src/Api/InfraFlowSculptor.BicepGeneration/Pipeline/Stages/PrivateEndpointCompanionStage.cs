using InfraFlowSculptor.BicepGeneration.Generators;
using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 540 — Private endpoint companion module injection.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.WorkItems"/> populated (stage 300),
/// <see cref="NetworkingResolutionStage"/> has run (stage 520).</para>
/// <para><b>Post-conditions:</b> for each work item whose resource is privatized, a companion
/// PE work item is appended to the pipeline context. The companion module creates private endpoints
/// and DNS zone groups per resource.</para>
/// <para>No-op when no resources are privatized.</para>
/// </remarks>
public sealed class PrivateEndpointCompanionStage : IBicepGenerationStage
{
    private readonly PrivateEndpointTypeBicepGenerator _peGenerator = new();

    /// <inheritdoc />
    public int Order => 540;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var privatizedItems = context.WorkItems
            .Where(item => item.Resource.IsPrivatized && item.Resource.PrivateEndpointConfig is not null)
            .ToList();

        if (privatizedItems.Count == 0)
            return;

        var companionWorkItems = new List<ModuleWorkItem>();

        foreach (var item in privatizedItems)
        {
            var peConfig = item.Resource.PrivateEndpointConfig!;
            var resourceTypeName = item.Spec.ResourceTypeName;

            if (!PrivateEndpointGroupIdCatalog.GroupIdsByResourceType.TryGetValue(resourceTypeName, out var groupIds))
                continue;

            var resourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(item.Resource.Name);
            var peModuleName = $"pe{Capitalize(resourceIdentifier)}";

            // Build the PE companion spec inline — it references the parent resource output
            var peSpec = BuildPrivateEndpointSpec(peModuleName, item, groupIds, peConfig.SubnetName);

            var peModule = new GeneratedTypeModule
            {
                ModuleName = peModuleName,
                ModuleFileName = peModuleName,
                ModuleFolderName = "PrivateEndpoints",
                ResourceTypeName = "PrivateEndpoint",
                ResourceGroupName = item.Module.ResourceGroupName,
                LogicalResourceName = $"pe-{item.Resource.Name}",
            };

            companionWorkItems.Add(new ModuleWorkItem
            {
                Resource = item.Resource,
                Spec = peSpec,
                Module = peModule,
            });
        }

        context.WorkItems.AddRange(companionWorkItems);
    }

    private static BicepModuleSpec BuildPrivateEndpointSpec(
        string moduleName,
        ModuleWorkItem parentItem,
        IReadOnlyList<string> groupIds,
        string subnetName)
    {
        var parentResourceName = parentItem.Resource.Name;
        var parentModuleName = parentItem.Module.ModuleName;

        return new BicepModuleBuilder()
            .Module(moduleName, "PrivateEndpoints", "PrivateEndpoint")
            .Param("location", BicepType.String, "Azure region for the Private Endpoint")
            .Param("privateEndpointName", BicepType.String, "Name of the Private Endpoint")
            .Param("privateLinkServiceId", BicepType.String, "Resource ID of the service to connect to")
            .Param("subnetId", BicepType.String, "Resource ID of the subnet for the Private Endpoint")
            .Param("privateDnsZoneId", BicepType.String,
                "Resource ID of the Private DNS Zone for DNS integration",
                defaultValue: new BicepStringLiteral(""))
            .Resource("privateEndpoint", "Microsoft.Network/privateEndpoints@2023-11-01")
            .Property("name", new BicepReference("privateEndpointName"))
            .Property("location", new BicepReference("location"))
            .Property("properties", props => props
                .Property("privateLinkServiceConnections", new BicepArrayExpression([
                    new BicepObjectExpression([
                        new BicepPropertyAssignment("name", new BicepReference("privateEndpointName")),
                        new BicepPropertyAssignment("properties", new BicepObjectExpression([
                            new BicepPropertyAssignment("privateLinkServiceId", new BicepReference("privateLinkServiceId")),
                            new BicepPropertyAssignment("groupIds", new BicepArrayExpression(
                                groupIds.Select(g => (BicepExpression)new BicepStringLiteral(g)).ToList())),
                        ])),
                    ]),
                ]))
                .Property("subnet", subnet => subnet
                    .Property("id", new BicepReference("subnetId"))))
            .AdditionalResource("dnsZoneGroup", "Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-11-01",
                parentSymbol: "privateEndpoint",
                condition: new BicepRawExpression("!empty(privateDnsZoneId)"),
                bodyBuilder: body => body
                    .Property("name", new BicepStringLiteral("default"))
                    .Property("properties", props => props
                        .Property("privateDnsZoneConfigs", new BicepArrayExpression([
                            new BicepObjectExpression([
                                new BicepPropertyAssignment("name", new BicepStringLiteral("config")),
                                new BicepPropertyAssignment("properties", new BicepObjectExpression([
                                    new BicepPropertyAssignment("privateDnsZoneId", new BicepReference("privateDnsZoneId")),
                                ])),
                            ]),
                        ]))))
            .Output("privateEndpointId", BicepType.String, new BicepRawExpression("privateEndpoint.id"),
                description: "Resource ID of the created Private Endpoint")
            .Build();
    }

    private static string Capitalize(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
