namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 540 — Private endpoint companion module injection.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.WorkItems"/> populated (stage 300),
/// <see cref="NetworkingResolutionStage"/> has run (stage 520).</para>
/// <para><b>Post-conditions:</b> for each work item whose resource is privatized, a companion
/// <c>private-resources.bicep</c> module is generated and referenced from <c>main.bicep</c>.
/// The companion creates private endpoints, DNS zone groups, and optional private DNS zones.</para>
/// <para>No-op when no resources are privatized.</para>
/// </remarks>
public sealed class PrivateEndpointCompanionStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 540;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var profile = context.Request.NetworkingProfile;
        if (profile is null)
            return;

        var privatizedItems = context.WorkItems
            .Where(item => item.Resource.IsPrivatized)
            .ToList();

        if (privatizedItems.Count == 0)
            return;

        // TODO: For each privatized resource, generate a companion private-resources.bicep
        // using the PrivateEndpointTypeBicepGenerator as a building block.
        // The companion module accepts perResourceNetworkConfig and creates:
        // - Private Endpoint(s) with group IDs from PrivateEndpointGroupIdCatalog
        // - DNS Zone Group linking to private DNS zones
        // - (if AutoManaged) Private DNS Zone + VNet Link creation
    }
}
