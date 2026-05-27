using InfraFlowSculptor.BicepGeneration.Helpers;

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

    private const string AcrPullIdentityIdPropertyName = "acrPullIdentityId";

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

        // Resources with an acrPullIdentityId property also need user identity for ACR registry auth.
        AddAcrPullIdentityResources(request.Resources, userIdentityResources);

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

    /// <summary>
    /// Registers resources that specify an <c>acrPullIdentityId</c> property as needing
    /// user-assigned identity, even if no role assignment exists on the resource itself.
    /// </summary>
    private static void AddAcrPullIdentityResources(
        IEnumerable<ResourceDefinition> resources,
        Dictionary<(string Name, string Type), List<string>> userIdentityResources)
    {
        var resourceList = resources as IReadOnlyList<ResourceDefinition> ?? resources.ToList();

        foreach (var resource in resourceList)
        {
            if (!resource.Properties.TryGetValue(AcrPullIdentityIdPropertyName, out var acrPullIdentityIdStr)
                || string.IsNullOrWhiteSpace(acrPullIdentityIdStr)
                || !Guid.TryParse(acrPullIdentityIdStr, out var acrPullIdentityId))
            {
                continue;
            }

            // Find the UAI resource to get its name for the Bicep identifier.
            var uaiResource = resourceList.FirstOrDefault(r =>
                r.ResourceId == acrPullIdentityId
                && r.Type == "Microsoft.ManagedIdentity/userAssignedIdentities");

            if (uaiResource is null)
                continue;

            var key = (resource.Name, resource.Type);
            var uaiBicepId = BicepIdentifierHelper.ToBicepIdentifier(uaiResource.Name);

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
    }
}
