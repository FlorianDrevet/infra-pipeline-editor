using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using PipelineVariableGroup = InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.PipelineVariableGroup;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Infrastructure Configuration aggregate.</summary>
    public static class InfrastructureConfig
    {
        /// <summary>Returned when an infrastructure configuration with the specified identifier does not exist.</summary>
        public static Error NotFoundError(InfrastructureConfigId id) => Error.NotFound(
            code: "InfrastructureConfig.NotFound",
            description: $"A InfrastructureConfig with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returned when the caller lacks sufficient permissions on the configuration.</summary>
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

        /// <summary>Returns an error when a pipeline variable group with the same name already exists.</summary>
        public static Error DuplicateVariableGroup(string groupName) => Error.Conflict(
            code: "InfrastructureConfig.DuplicateVariableGroup",
            description: $"A pipeline variable group named '{groupName}' already exists in this configuration."
        );

        /// <summary>Returns an error when a pipeline variable group is not found.</summary>
        public static Error VariableGroupNotFound(PipelineVariableGroup.PipelineVariableGroupId groupId) => Error.NotFound(
            code: "InfrastructureConfig.VariableGroupNotFound",
            description: $"Pipeline variable group '{groupId}' was not found."
        );

        /// <summary>Returns an error when a duplicate Bicep parameter mapping already exists in this group.</summary>
        public static Error DuplicateVariableMapping(string bicepParameterName) => Error.Conflict(
            code: "InfrastructureConfig.DuplicateVariableMapping",
            description: $"A mapping for Bicep parameter '{bicepParameterName}' already exists in this variable group."
        );

        /// <summary>Returns an error when a variable mapping is not found in the group.</summary>
        public static Error VariableMappingNotFound(PipelineVariableGroup.PipelineVariableMappingId mappingId) => Error.NotFound(
            code: "InfrastructureConfig.VariableMappingNotFound",
            description: $"Variable mapping '{mappingId}' was not found."
        );
    }
}