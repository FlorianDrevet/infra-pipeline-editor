namespace InfraFlowSculptor.Contracts.RoleAssignments.Responses;

/// <summary>Represents an Azure built-in RBAC role definition applicable to a resource type.</summary>
/// <param name="Id">Azure role definition ID (e.g. <c>/providers/Microsoft.Authorization/roleDefinitions/…</c>).</param>
/// <param name="Name">Display name of the role (e.g. "Key Vault Secrets Officer").</param>
/// <param name="Description">Short description of the permissions granted by the role.</param>
/// <param name="DocumentationUrl">Link to the official Azure documentation page for this role.</param>
public record AzureRoleDefinitionResponse(
    string Id,
    string Name,
    string Description,
    string DocumentationUrl
);
