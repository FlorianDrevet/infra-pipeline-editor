using InfraFlowSculptor.BicepGeneration.Ir;
using InfraFlowSculptor.BicepGeneration.Ir.Builder;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 520 — Networking resolution module generation.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.WorkItems"/> populated (stage 300),
/// <see cref="BicepGenerationContext.Request"/> contains at least one privatized resource
/// with <see cref="GenerationCore.Models.PrivateEndpointDefinition"/>.</para>
/// <para><b>Post-conditions:</b> a synthetic <c>networking/resolution.bicep</c> module is appended to
/// the context's work items, resolving VNet/subnet references and producing outputs
/// consumed by companion private endpoint modules.</para>
/// <para>No-op when no resources are privatized.</para>
/// </remarks>
public sealed class NetworkingResolutionStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 520;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var privatizedResources = context.WorkItems
            .Where(item => item.Resource.IsPrivatized && item.Resource.PrivateEndpointConfig is not null)
            .ToList();

        if (privatizedResources.Count == 0)
            return;

        // Collect distinct VNet/subnet combinations referenced by privatized resources
        var distinctSubnets = privatizedResources
            .Select(item => item.Resource.PrivateEndpointConfig!)
            .DistinctBy(pe => (pe.VirtualNetworkId, pe.SubnetName))
            .ToList();

        // Generate a networking resolution module that resolves existing VNet/subnet references
        var spec = BuildNetworkingResolutionSpec(distinctSubnets);

        var module = new GeneratedTypeModule
        {
            ModuleName = "networkingResolution",
            ModuleFileName = "networkingResolution",
            ModuleFolderName = "Networking",
            ResourceTypeName = "NetworkingResolution",
            ResourceGroupName = string.Empty,
            LogicalResourceName = "networking-resolution",
        };

        context.WorkItems.Add(new ModuleWorkItem
        {
            Resource = new GenerationCore.Models.ResourceDefinition
            {
                Name = "networking-resolution",
                Type = "Synthetic/NetworkingResolution",
                IsPrivatized = false,
            },
            Spec = spec,
            Module = module,
        });
    }

    private static BicepModuleSpec BuildNetworkingResolutionSpec(
        List<GenerationCore.Models.PrivateEndpointDefinition> subnets)
    {
        var builder = new BicepModuleBuilder()
            .Module("networkingResolution", "Networking", "NetworkingResolution")
            .Resource("peSubnet", "Microsoft.Network/virtualNetworks/subnets@2024-05-01")
            .Param("vnetResourceId", BicepType.String, "Resource ID of the Virtual Network used for Private Endpoints")
            .Param("peSubnetName", BicepType.String, "Name of the subnet dedicated to Private Endpoints")
            .ExistingResource("vnet", "Microsoft.Network/virtualNetworks@2024-05-01", "last(split(vnetResourceId, '/'))");

        // Output the subnet resource ID for consumption by PE companion modules
        builder.Output("peSubnetId", BicepType.String,
            new BicepRawExpression("peSubnet.id"),
            description: "Full resource ID of the PE subnet");

        return builder.Build();
    }
}
