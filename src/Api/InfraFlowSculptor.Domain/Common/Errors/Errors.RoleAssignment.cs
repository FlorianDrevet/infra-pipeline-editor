using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class RoleAssignment
    {
        public static Error SourceResourceNotFound(AzureResourceId id) => Error.NotFound(
            code: "RoleAssignment.SourceResourceNotFound",
            description: $"A resource with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error TargetResourceNotFound(AzureResourceId id) => Error.NotFound(
            code: "RoleAssignment.TargetResourceNotFound",
            description: $"The target resource with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error NotFound(RoleAssignmentId id) => Error.NotFound(
            code: "RoleAssignment.NotFound",
            description: $"A role assignment with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error InvalidRoleDefinitionForResourceType(string roleDefinitionId, string resourceType) =>
            Error.Validation(
                code: "RoleAssignment.InvalidRoleDefinition",
                description: $"The role definition '{roleDefinitionId}' is not applicable to resource type '{resourceType}'. " +
                             $"Use GET /azure-resources/{{resourceId}}/role-assignments/available-role-definitions to see the supported roles.",
                metadata: new Dictionary<string, object>
                {
                    { "RoleDefinitionId", roleDefinitionId },
                    { "ResourceType", resourceType },
                }
            );

        /// <summary>Error returned when a User-Assigned Identity is required but not specified.</summary>
        public static Error UserAssignedIdentityRequired() => Error.Validation(
            code: "RoleAssignment.UserAssignedIdentityRequired",
            description: "A User-Assigned Identity must be specified when using UserAssigned managed identity type.");

        /// <summary>Error returned when the specified User-Assigned Identity resource does not exist.</summary>
        public static Error UserAssignedIdentityNotFound(AzureResourceId id) => Error.NotFound(
            code: "RoleAssignment.UserAssignedIdentityNotFound",
            description: $"User-Assigned Identity '{id.Value}' was not found.");
    }
}
