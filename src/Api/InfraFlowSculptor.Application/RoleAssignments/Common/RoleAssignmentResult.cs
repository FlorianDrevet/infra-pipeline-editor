using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RoleAssignments.Common;

public record RoleAssignmentResult(
    RoleAssignmentId Id,
    AzureResourceId SourceResourceId,
    AzureResourceId TargetResourceId,
    ManagedIdentityType ManagedIdentityType,
    string RoleDefinitionId
);
