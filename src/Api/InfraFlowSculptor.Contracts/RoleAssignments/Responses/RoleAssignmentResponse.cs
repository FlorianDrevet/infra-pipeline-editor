namespace InfraFlowSculptor.Contracts.RoleAssignments.Responses;

public record RoleAssignmentResponse(
    Guid Id,
    Guid SourceResourceId,
    Guid TargetResourceId,
    string ManagedIdentityType,
    string RoleDefinitionId
);
