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
        IReadOnlyCollection<RoleAssignmentDefinition> roleAssignments)
    {
        return roleAssignments
            .GroupBy(ra => (ra.SourceResourceName, ra.TargetResourceName, ra.ManagedIdentityType,
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
                    Roles = g.Select(ra => new RoleRef(ra.RoleDefinitionName)).ToList()
                };
            })
            .ToList();
    }

    /// <summary>
    /// Resolves the Bicep expression for the <c>principalId</c> parameter
    /// of a role assignment module based on the managed identity type.
    /// </summary>
    internal static string ResolvePrincipalIdExpression(
        GroupedRoleAssignment group,
        IReadOnlyCollection<GeneratedTypeModule> modules)
    {
        if (group.ManagedIdentityType == "UserAssigned" && group.UserAssignedIdentityName is not null)
        {
            var uaiIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.UserAssignedIdentityName);
            return $"userAssignedIdentity{BicepFormattingHelper.Capitalize(uaiIdentifier)}Module.outputs.principalId";
        }

        // SystemAssigned: reference the source resource module's principalId output
        var sourceIdentifier = BicepIdentifierHelper.ToBicepIdentifier(group.SourceResourceName);
        var sourceBaseName = ResourceTypeMetadata.GetBaseModuleName(group.SourceResourceType);
        return $"{sourceBaseName}{BicepFormattingHelper.Capitalize(sourceIdentifier)}Module.outputs.principalId";
    }
}
