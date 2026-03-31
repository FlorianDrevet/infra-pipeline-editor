using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.Entites;

namespace InfraFlowSculptor.Application.RoleAssignments.Common;

/// <summary>Analyzes the impact of removing a role assignment from a source resource.</summary>
public interface IRoleAssignmentImpactAnalyzer
{
    /// <summary>
    /// Evaluates what would break if <paramref name="roleAssignment"/> were removed from <paramref name="sourceResource"/>.
    /// </summary>
    /// <param name="sourceResource">The source resource that owns the role assignment, loaded with RoleAssignments and AppSettings.</param>
    /// <param name="roleAssignment">The role assignment being removed.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A list of impact items, empty when no impact is detected.</returns>
    Task<List<RoleAssignmentImpactItem>> AnalyzeAsync(
        AzureResource sourceResource,
        RoleAssignment roleAssignment,
        CancellationToken cancellationToken);
}
