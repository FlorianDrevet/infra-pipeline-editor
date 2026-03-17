namespace InfraFlowSculptor.Application.RoleAssignments.Common;

public record AzureRoleDefinitionResult(
    string Id,
    string Name,
    string Description,
    string DocumentationUrl
);
