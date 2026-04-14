using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.AnalyzeRoleAssignmentImpact;

/// <summary>Query to analyze the impact of removing a role assignment before deletion.</summary>
/// <param name="SourceResourceId">Identifier of the source resource that owns the role assignment.</param>
/// <param name="RoleAssignmentId">Identifier of the role assignment to analyze.</param>
public record AnalyzeRoleAssignmentImpactQuery(
    AzureResourceId SourceResourceId,
    RoleAssignmentId RoleAssignmentId
) : IQuery<RoleAssignmentImpactResult>;
