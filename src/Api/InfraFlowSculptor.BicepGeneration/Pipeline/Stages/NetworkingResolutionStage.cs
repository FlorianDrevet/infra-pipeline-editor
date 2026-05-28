namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 520 — Networking resolution module generation.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.WorkItems"/> populated (stage 300),
/// <see cref="BicepGenerationContext.Request"/> contains a non-null
/// <see cref="GenerationCore.Models.NetworkingProfileDefinition"/>.</para>
/// <para><b>Post-conditions:</b> a synthetic <c>networking/resolution.bicep</c> module is appended to
/// the context's output artifacts, resolving VNet/subnet references and producing typed outputs
/// consumed by companion private endpoint modules.</para>
/// <para>No-op when <c>NetworkingProfile</c> is <c>null</c> or no resources are privatized.</para>
/// </remarks>
public sealed class NetworkingResolutionStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 520;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var profile = context.Request.NetworkingProfile;
        if (profile is null)
            return;

        var privatizedResources = context.WorkItems
            .Where(item => item.Resource.IsPrivatized)
            .ToList();

        if (privatizedResources.Count == 0)
            return;

        // TODO: Generate networking/types.bicep and networking/resolution.bicep IR modules
        // The resolution module resolves VNet/subnet references (existing or create-new)
        // and outputs typed per-resource network config consumed by companion PE modules.
    }
}
