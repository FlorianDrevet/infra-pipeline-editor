using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Queries.ListRoleAssignmentsByIdentity;

/// <summary>
/// Query to list all role assignments that use a specific User-Assigned Identity.
/// </summary>
/// <param name="IdentityId">The <see cref="AzureResourceId"/> of the User-Assigned Identity.</param>
public record ListRoleAssignmentsByIdentityQuery(AzureResourceId IdentityId)
    : IQuery<List<IdentityRoleAssignmentResult>>;
