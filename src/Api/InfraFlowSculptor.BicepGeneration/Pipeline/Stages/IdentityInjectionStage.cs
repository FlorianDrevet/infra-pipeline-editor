using InfraFlowSculptor.BicepGeneration.Ir.Transformations;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 400 — Identity injection.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.Identity"/> and
/// <see cref="BicepGenerationContext.WorkItems"/> populated (stages 100 + 300).</para>
/// <para><b>Post-conditions:</b> for each work item, the module Bicep content carries either a
/// hardcoded identity block (uniform ARM type) or a parameterized identity block (mixed ARM type),
/// and <see cref="ModuleWorkItem.IdentityKind"/> / <see cref="ModuleWorkItem.UsesParameterizedIdentity"/>
/// are set accordingly. The spec's exported types include <c>ManagedIdentityType</c> for parameterized resources.</para>
/// </remarks>
public sealed class IdentityInjectionStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 400;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var identity = context.Identity;

        foreach (var item in context.WorkItems)
        {
            var resource = item.Resource;
            var key = (resource.Name, resource.Type);
            var needsSystem = identity.SystemIdentityResources.Contains(key);
            identity.UserIdentityResources.TryGetValue(key, out var uaiIdentifiers);
            var needsUser = resource.Type != "Microsoft.ManagedIdentity/userAssignedIdentities"
                && uaiIdentifiers is { Count: > 0 };
            var isMixed = identity.MixedIdentityArmTypes.Contains(resource.Type);

            string? identityKind = null;
            if (needsSystem && needsUser)
                identityKind = "SystemAssigned, UserAssigned";
            else if (needsSystem)
                identityKind = "SystemAssigned";
            else if (needsUser)
                identityKind = "UserAssigned";

            item.IdentityKind = identityKind;
            item.UsesParameterizedIdentity = isMixed;

            if (isMixed)
            {
                var hasAnyUaiForType = identity.UserIdentityResources
                    .Any(kv => kv.Key.Type == resource.Type);
                item.Spec = item.Spec.WithParameterizedIdentity(hasAnyUaiForType);
            }
            else
            {
                if (needsSystem)
                    item.Spec = item.Spec.WithSystemAssignedIdentity();

                if (needsUser)
                    item.Spec = item.Spec.WithUserAssignedIdentity(needsSystem);
            }
        }
    }
}
