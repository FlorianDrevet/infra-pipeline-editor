namespace InfraFlowSculptor.Contracts.RoleAssignments.Responses;

public record AzureRoleDefinitionResponse(
    string Id,
    string Name,
    string Description,
    string DocumentationUrl
);
