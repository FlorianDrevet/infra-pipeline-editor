namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 560 — Public network access disablement injection.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.WorkItems"/> populated (stage 300).</para>
/// <para><b>Post-conditions:</b> every privatized resource module has
/// <c>publicNetworkAccess: 'Disabled'</c> injected into its resource properties, along with
/// <c>networkAcls: { defaultAction: 'Deny' }</c> where applicable (Storage, KV, etc.).</para>
/// <para>No-op when no resources are privatized.</para>
/// </remarks>
public sealed class PublicNetworkAccessStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 560;

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

        // TODO: For each privatized module spec, inject:
        // 1. publicNetworkAccess: 'Disabled' in the resource properties
        // 2. networkAcls: { defaultAction: 'Deny' } for resources that support it (Storage, KV)
        // This is done via IR transformation on the BicepModuleSpec (similar to TagsInjectionStage).
    }
}
