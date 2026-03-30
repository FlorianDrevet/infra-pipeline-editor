using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="ProjectAggregate.Project"/> aggregate.</summary>
    public static class Project
    {
        private const string NotFoundCode = "Project.NotFound";
        private const string ForbiddenCode = "Project.Forbidden";
        private const string MemberAlreadyExistsCode = "Project.MemberAlreadyExists";
        private const string CannotRemoveOwnerCode = "Project.CannotRemoveOwner";
        private const string MemberNotFoundCode = "Project.MemberNotFound";
        private const string DuplicateVariableGroupCode = "Project.DuplicateVariableGroup";
        private const string VariableGroupNotFoundCode = "Project.VariableGroupNotFound";
        private const string DuplicateVariableMappingCode = "Project.DuplicateVariableMapping";
        private const string VariableMappingNotFoundCode = "Project.VariableMappingNotFound";

        /// <summary>Returns a not-found error for the given project identifier.</summary>
        public static Error NotFoundError(ProjectId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No Project with id {id} was found.");

        /// <summary>Returns a forbidden error when the caller lacks write access.</summary>
        public static Error ForbiddenError() =>
            Error.Forbidden(code: ForbiddenCode, description: "You do not have permission to modify this Project.");

        /// <summary>Returns an error when the user is already a member of the project.</summary>
        public static Error MemberAlreadyExistsError() =>
            Error.Conflict(code: MemberAlreadyExistsCode, description: "The user is already a member of this project.");

        /// <summary>Returns an error when trying to remove the project owner.</summary>
        public static Error CannotRemoveOwnerError() =>
            Error.Conflict(code: CannotRemoveOwnerCode, description: "Cannot remove the owner from a project.");

        /// <summary>Returns an error when the target member is not found in the project.</summary>
        public static Error MemberNotFoundError() =>
            Error.NotFound(code: MemberNotFoundCode, description: "The specified user is not a member of this project.");

        /// <summary>Returns an error when a pipeline variable group with the same name already exists.</summary>
        public static Error DuplicateVariableGroupError(string groupName) =>
            Error.Conflict(code: DuplicateVariableGroupCode, description: $"A pipeline variable group named '{groupName}' already exists in this project.");

        /// <summary>Returns a not-found error for the given variable group identifier.</summary>
        public static Error VariableGroupNotFoundError(ProjectPipelineVariableGroupId groupId) =>
            Error.NotFound(code: VariableGroupNotFoundCode, description: $"No pipeline variable group with id '{groupId}' was found in this project.");

        /// <summary>Returns an error when a mapping with the same Bicep parameter already exists.</summary>
        public static Error DuplicateVariableMappingError(string bicepParameterName) =>
            Error.Conflict(code: DuplicateVariableMappingCode, description: $"A mapping for Bicep parameter '{bicepParameterName}' already exists in this group.");

        /// <summary>Returns a not-found error for the given variable mapping identifier.</summary>
        public static Error VariableMappingNotFoundError(ProjectPipelineVariableMappingId mappingId) =>
            Error.NotFound(code: VariableMappingNotFoundCode, description: $"No pipeline variable mapping with id '{mappingId}' was found in this group.");

        /// <summary>Returned when a role string cannot be parsed into a valid role enum value.</summary>
        public static Error InvalidRoleError(string role) =>
            Error.Validation(code: "Project.InvalidRole", description: $"Invalid role: {role}");

        /// <summary>Returned when a repository mode string cannot be parsed into a valid mode enum value.</summary>
        public static Error InvalidRepositoryModeError(string mode) =>
            Error.Validation(code: "Project.InvalidRepositoryMode", description: $"Invalid repository mode '{mode}'. Valid values: MultiRepo, MonoRepo.");

        /// <summary>Returned when a project has no infrastructure configurations for generation.</summary>
        public static Error NoConfigurationsError() =>
            Error.NotFound(code: "Project.NoConfigurations", description: "No infrastructure configurations found for this project.");

        /// <summary>Returned when no generated Bicep files exist for the given project.</summary>
        public static Error BicepFilesNotFoundError(Guid projectId) =>
            Error.NotFound(code: "Project.BicepFilesNotFound", description: $"No generated Bicep files found for project '{projectId}'.");

        /// <summary>Returned when a specific Bicep file is not found in the latest project generation.</summary>
        public static Error BicepFileNotFoundError(string filePath) =>
            Error.NotFound(code: "Project.BicepFileNotFound", description: $"File '{filePath}' was not found.");

        /// <summary>Returned when no generated pipeline files exist for the given project.</summary>
        public static Error PipelineFilesNotFoundError(Guid projectId) =>
            Error.NotFound(code: "Project.PipelineFilesNotFound", description: $"No generated pipeline files found for project '{projectId}'.");

        /// <summary>Returned when a specific pipeline file is not found in the latest project generation.</summary>
        public static Error PipelineFileNotFoundError(string filePath) =>
            Error.NotFound(code: "Project.PipelineFileNotFound", description: $"File '{filePath}' was not found.");
    }
}
