using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class InfrastructureConfig
    {
        public static Error NotFoundError(InfrastructureConfigId id) => Error.NotFound(
            code: "InfrastructureConfig.NotFound",
            description: $"A InfrastructureConfig with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        public static Error ForbiddenError() => Error.Forbidden(
            code: "InfrastructureConfig.Forbidden",
            description: "You do not have sufficient permissions to perform this action on this configuration."
        );

        /// <summary>Returns an error when trying to add a duplicate cross-config reference.</summary>
        public static Error DuplicateCrossConfigReference(AzureResourceId targetResourceId) => Error.Conflict(
            code: "InfrastructureConfig.DuplicateCrossConfigReference",
            description: $"A cross-config reference to resource '{targetResourceId}' already exists."
        );

        /// <summary>Returns an error when a cross-config reference is not found.</summary>
        public static Error CrossConfigReferenceNotFound(CrossConfigResourceReferenceId referenceId) => Error.NotFound(
            code: "InfrastructureConfig.CrossConfigReferenceNotFound",
            description: $"Cross-config reference '{referenceId}' was not found."
        );

        /// <summary>Returns an error when trying to reference a resource in the same configuration.</summary>
        public static Error CannotReferenceSameConfig() => Error.Validation(
            code: "InfrastructureConfig.CannotReferenceSameConfig",
            description: "Cannot add a cross-config reference to a resource in the same configuration."
        );

        /// <summary>Returns an error when the target resource does not exist.</summary>
        public static Error TargetResourceNotFound(AzureResourceId targetResourceId) => Error.NotFound(
            code: "InfrastructureConfig.TargetResourceNotFound",
            description: $"Target resource '{targetResourceId}' was not found in the project."
        );

        /// <summary>Returns an error when the target resource belongs to a different project.</summary>
        public static Error TargetResourceNotInSameProject() => Error.Validation(
            code: "InfrastructureConfig.TargetResourceNotInSameProject",
            description: "The target resource must belong to a configuration within the same project."
        );
    }
}