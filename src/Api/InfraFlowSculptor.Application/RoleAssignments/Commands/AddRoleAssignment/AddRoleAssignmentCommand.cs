using ErrorOr;
using InfraFlowSculptor.Application.RoleAssignments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;

/// <summary>Command to add an RBAC role assignment from a source resource to a target resource.</summary>
/// <param name="SourceResourceId">Identifier of the source Azure resource.</param>
/// <param name="TargetResourceId">Identifier of the target Azure resource.</param>
/// <param name="ManagedIdentityType">Type of managed identity to use.</param>
/// <param name="RoleDefinitionId">Azure role definition ID to grant.</param>
/// <param name="UserAssignedIdentityId">Optional User-Assigned Identity resource ID (required when ManagedIdentityType is UserAssigned).</param>
public record AddRoleAssignmentCommand(
    AzureResourceId SourceResourceId,
    AzureResourceId TargetResourceId,
    string ManagedIdentityType,
    string RoleDefinitionId,
    AzureResourceId? UserAssignedIdentityId
) : IRequest<ErrorOr<RoleAssignmentResult>>;
