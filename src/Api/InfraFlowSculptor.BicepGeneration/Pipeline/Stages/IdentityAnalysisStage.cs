using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.GenerationCore.Models;

namespace InfraFlowSculptor.BicepGeneration.Pipeline.Stages;

/// <summary>
/// Stage 100 — Pre-build identity analysis.
/// </summary>
/// <remarks>
/// <para><b>Pre-conditions:</b> <see cref="BicepGenerationContext.Request"/> populated.</para>
/// <para><b>Post-conditions:</b> <see cref="BicepGenerationContext.Identity"/> populated with
/// system / user identity assignments and the set of ARM types whose instances require
/// mixed identity kinds (parameterized injection).</para>
/// </remarks>
public sealed class IdentityAnalysisStage : IBicepGenerationStage
{
    /// <inheritdoc />
    public int Order => 100;

    /// <inheritdoc />
    public void Execute(BicepGenerationContext context)
    {
        var request = context.Request;

        var systemIdentityResources = request.RoleAssignments
            .Where(ra => ra.ManagedIdentityType == "SystemAssigned")
            .Select(ra => (ra.SourceResourceName, ra.SourceResourceType))
            .ToHashSet();

        var userIdentityResources = request.RoleAssignments
            .Where(ra => ra.ManagedIdentityType == "UserAssigned" && ra.UserAssignedIdentityName is not null)
            .GroupBy(ra => (ra.SourceResourceName, ra.SourceResourceType))
            .ToDictionary(
                g => g.Key,
                g => g.Select(ra => BicepIdentifierHelper.ToBicepIdentifier(ra.UserAssignedIdentityName!))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList());

        // Resources with explicitly assigned UAI (even without UA role assignments) also need user identity.
        foreach (var resource in request.Resources)
        {
            if (resource.AssignedUserAssignedIdentityName is null)
                continue;

            var key = (resource.Name, resource.Type);
            var uaiBicepId = BicepIdentifierHelper.ToBicepIdentifier(resource.AssignedUserAssignedIdentityName);
            if (userIdentityResources.TryGetValue(key, out var existingList))
            {
                if (!existingList.Contains(uaiBicepId, StringComparer.OrdinalIgnoreCase))
                    existingList.Add(uaiBicepId);
            }
            else
            {
                userIdentityResources[key] = [uaiBicepId];
            }
        }

        var identityKindsByArmType = ComputeIdentityKindsByArmType(
            request.Resources, systemIdentityResources, userIdentityResources);

        var mixedIdentityArmTypes = identityKindsByArmType
            .Where(kv => kv.Value.Count > 1)
            .Select(kv => kv.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        context.Identity = new IdentityAnalysisResult(
            systemIdentityResources,
            userIdentityResources,
            mixedIdentityArmTypes);
    }

    private static Dictionary<string, HashSet<string>> ComputeIdentityKindsByArmType(
        IEnumerable<ResourceDefinition> resources,
        HashSet<(string Name, string Type)> systemIdentityResources,
        Dictionary<(string Name, string Type), List<string>> userIdentityResources)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var resource in resources)
        {
            if (resource.Type == "Microsoft.ManagedIdentity/userAssignedIdentities")
                continue;

            var key = (resource.Name, resource.Type);
            var hasSystem = systemIdentityResources.Contains(key);
            var hasUser = userIdentityResources.ContainsKey(key);

            string? kind = null;
            if (hasSystem && hasUser)
                kind = "SystemAssigned, UserAssigned";
            else if (hasSystem)
                kind = "SystemAssigned";
            else if (hasUser)
                kind = "UserAssigned";

            if (kind is null) continue;

            if (!result.TryGetValue(resource.Type, out var kindSet))
            {
                kindSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                result[resource.Type] = kindSet;
            }
            kindSet.Add(kind);
        }

        return result;
    }
}
