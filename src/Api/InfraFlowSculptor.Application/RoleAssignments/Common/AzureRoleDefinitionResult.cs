namespace InfraFlowSculptor.Application.RoleAssignments.Common;

/// <summary>Represents a role definition returned by the available-role-definitions query.</summary>
/// <param name="Id">Azure role definition identifier.</param>
/// <param name="Name">Display name of the role definition.</param>
/// <param name="Description">Short description of the granted permissions.</param>
/// <param name="DocumentationUrl">Link to the official Azure documentation for the role.</param>
/// <param name="RequiresUserAssignedIdentity">Indicates whether the role can only be granted through a user-assigned identity.</param>
public record AzureRoleDefinitionResult(
    string Id,
    string Name,
    string Description,
    string DocumentationUrl,
    bool RequiresUserAssignedIdentity
);
