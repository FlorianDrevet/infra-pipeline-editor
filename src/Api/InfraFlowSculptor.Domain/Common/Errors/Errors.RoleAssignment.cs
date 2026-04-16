using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to role assignments.</summary>
    public static class RoleAssignment
    {
        /// <summary>Returned when the source resource of a role assignment does not exist.</summary>
        public static Error SourceResourceNotFound(AzureResourceId id) => Error.NotFound(
            code: "RoleAssignment.SourceResourceNotFound",
            description: $"A resource with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returned when the target resource of a role assignment does not exist.</summary>
        public static Error TargetResourceNotFound(AzureResourceId id) => Error.NotFound(
            code: "RoleAssignment.TargetResourceNotFound",
            description: $"The target resource with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returned when a role assignment with the specified identifier does not exist.</summary>
        public static Error NotFound(RoleAssignmentId id) => Error.NotFound(
            code: "RoleAssignment.NotFound",
            description: $"A role assignment with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returned when a role definition is not applicable to the target resource type.</summary>
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

        /// <summary>Error returned when the managed identity type value is not recognized.</summary>
        public static Error InvalidManagedIdentityType(string value) => Error.Validation(
            code: "RoleAssignment.InvalidManagedIdentityType",
            description: $"The managed identity type '{value}' is not valid. " +
                         $"Allowed values: {string.Join(", ", Enum.GetNames<ManagedIdentityType.IdentityTypeEnum>())}.");

        /// <summary>Error returned when attempting to assign the AcrPull role with System Assigned identity.</summary>
        public static Error AcrPullRequiresUserAssigned() => Error.Validation(
            code: "RoleAssignment.AcrPullRequiresUserAssigned",
            description: "The AcrPull role can only be assigned using a User-Assigned Identity. System Assigned Identity is not supported for AcrPull.");
    }
}
