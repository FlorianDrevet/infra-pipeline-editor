using InfraFlowSculptor.BicepGeneration.Ir.Transformations;

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
        foreach (var item in context.WorkItems.Where(i => i.Resource.IsPrivatized))
        {
            item.Spec = item.Spec.WithPublicNetworkAccessDisabled(item.Spec.ResourceTypeName);
        }
    }
}
