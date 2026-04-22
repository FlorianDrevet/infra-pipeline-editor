using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to secure parameter mappings.</summary>
    public static class SecureParameterMapping
    {
        private const string NotFoundCode = "SecureParameterMapping.NotFound";
        private const string InconsistentMappingCode = "SecureParameterMapping.InconsistentMapping";
        private const string VariableGroupNotFoundCode = "SecureParameterMapping.VariableGroupNotFound";
        private const string ResourceNotFoundCode = "SecureParameterMapping.ResourceNotFound";

        /// <summary>Returned when no mapping exists for the specified secure parameter name.</summary>
        public static Error NotFound(string secureParameterName) => Error.NotFound(
            code: NotFoundCode,
            description: $"No mapping found for secure parameter '{secureParameterName}'.");

        /// <summary>Returned when variable group and pipeline variable name are not both set or both null.</summary>
        public static Error InconsistentMapping() => Error.Validation(
            code: InconsistentMappingCode,
            description: "Both variable group and pipeline variable name must be set or both must be null.");

        /// <summary>Returned when the specified variable group does not exist on the project.</summary>
        public static Error VariableGroupNotFound(ProjectPipelineVariableGroupId id) => Error.NotFound(
            code: VariableGroupNotFoundCode,
            description: $"The pipeline variable group with id '{id}' does not exist on the project.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } });

        /// <summary>Returned when the Azure resource does not exist.</summary>
        public static Error ResourceNotFound(AzureResourceId id) => Error.NotFound(
            code: ResourceNotFoundCode,
            description: $"The Azure resource with id '{id}' does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } });
    }
}
