using InfraFlowSculptor.BicepGeneration.Helpers;
using InfraFlowSculptor.BicepGeneration.Models;

namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Groups role assignments and resolves principal ID expressions for Bicep role assignment modules.
/// </summary>
internal static class RoleAssignmentAssembler
{
    /// <summary>
    /// Groups role assignments by (source, target, identity type) and collects
    /// the assigned roles into a single entry for each group.
    /// </summary>
    internal static List<GroupedRoleAssignment> GroupRoleAssignments(
        IReadOnlyList<RoleAssignmentDefinition> roleAssignments)
    {
        return roleAssignments
            .GroupBy(ra => (
                ra.SourceResourceName,
                ra.TargetResourceName,
                ra.TargetResourceType,
                ra.TargetResourceGroupName,
                ra.IsTargetCrossConfig,
                ra.ManagedIdentityType,
                ra.UserAssignedIdentityResourceId))
            .Select(g =>
            {
                var first = g.First();
                return new GroupedRoleAssignment
                {
                    SourceResourceName = first.SourceResourceName,
                    SourceResourceType = first.SourceResourceType,
                    TargetResourceName = first.TargetResourceName,
                    TargetResourceType = first.TargetResourceType,
                    TargetResourceTypeName = first.TargetResourceTypeName,
                    TargetResourceGroupName = first.TargetResourceGroupName,
                    TargetResourceAbbreviation = first.TargetResourceAbbreviation,
                    ServiceCategory = first.ServiceCategory,
                    ManagedIdentityType = first.ManagedIdentityType,
                    UserAssignedIdentityName = first.UserAssignedIdentityName,
                    IsTargetCrossConfig = first.IsTargetCrossConfig,
                    Roles = g.Select(ra => new RoleRef(ra.RoleDefinitionName, ra.ServiceCategory)).ToList()
                };
            })
            .ToList();
    }

    /// <summary>
    /// Resolves the Bicep expression for the <c>principalId</c> parameter
    /// of a role assignment module based on the managed identity type, and registers
    /// the corresponding <c>{symbol}Module.outputs.principalId</c> usage on
    /// <paramref name="tracker"/> so the output pruner can keep the output alive.
    /// </summary>
    internal static string ResolvePrincipalIdExpression(
        GroupedRoleAssignment group,
        IReadOnlyCollection<GeneratedTypeModule> modules,
        OutputUsageTracker tracker)
    {
        const string PrincipalIdOutputName = "principalId";

        if (group.ManagedIdentityType == "UserAssigned" && group.UserAssignedIdentityName is not null)
        {
            var uaiIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.UserAssignedIdentityName);
            var uaiSymbol = $"userAssignedIdentity{BicepFormattingHelper.Capitalize(uaiIdentifier)}Module";
            tracker.RegisterUsage(uaiSymbol, PrincipalIdOutputName);
            return $"{uaiSymbol}.outputs.{PrincipalIdOutputName}";
        }

        // SystemAssigned: reference the source resource module's principalId output
        var sourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.SourceResourceName);
        var sourceBaseName = ResourceTypeMetadata.GetBaseModuleName(group.SourceResourceType);
        var sourceSymbol = $"{sourceBaseName}{BicepFormattingHelper.Capitalize(sourceIdentifier)}Module";
        tracker.RegisterUsage(sourceSymbol, PrincipalIdOutputName);
        return $"{sourceSymbol}.outputs.{PrincipalIdOutputName}";
    }
}
