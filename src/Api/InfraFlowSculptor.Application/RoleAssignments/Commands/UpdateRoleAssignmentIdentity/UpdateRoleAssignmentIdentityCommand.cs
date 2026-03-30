using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.UpdateRoleAssignmentIdentity;

/// <summary>Command to update the managed identity on an existing role assignment.</summary>
/// <param name="SourceResourceId">Identifier of the source Azure resource.</param>
/// <param name="RoleAssignmentId">Identifier of the role assignment to update.</param>
/// <param name="ManagedIdentityType">New managed identity type.</param>
/// <param name="UserAssignedIdentityId">Optional User-Assigned Identity resource ID (required when ManagedIdentityType is UserAssigned).</param>
public record UpdateRoleAssignmentIdentityCommand(
    AzureResourceId SourceResourceId,
    RoleAssignmentId RoleAssignmentId,
    string ManagedIdentityType,
    AzureResourceId? UserAssignedIdentityId
) : ICommand<RoleAssignmentResult>;
