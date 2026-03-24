using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Common;

/// <summary>Application-layer result DTO for a role assignment.</summary>
/// <param name="Id">Unique identifier of the role assignment.</param>
/// <param name="SourceResourceId">Identifier of the source Azure resource.</param>
/// <param name="TargetResourceId">Identifier of the target Azure resource.</param>
/// <param name="ManagedIdentityType">Type of managed identity used.</param>
/// <param name="RoleDefinitionId">Azure role definition ID that was granted.</param>
/// <param name="UserAssignedIdentityId">Optional User-Assigned Identity resource ID.</param>
public record RoleAssignmentResult(
    RoleAssignmentId Id,
    AzureResourceId SourceResourceId,
    AzureResourceId TargetResourceId,
    ManagedIdentityType ManagedIdentityType,
    string RoleDefinitionId,
    AzureResourceId? UserAssignedIdentityId
);
