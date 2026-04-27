using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="ProjectAggregate.Entities.ProjectRepository"/> entity.</summary>
    public static class ProjectRepository
    {
        private const string InvalidAliasCode = "ProjectRepository.InvalidAlias";
        private const string DuplicateAliasCode = "ProjectRepository.DuplicateAlias";
        private const string NotFoundCode = "ProjectRepository.NotFound";
        private const string NoContentKindCode = "ProjectRepository.NoContentKind";
        private const string InvalidUrlCode = "ProjectRepository.InvalidUrl";
        private const string RepositoryInUseCode = "ProjectRepository.RepositoryInUse";

        /// <summary>Returns a validation error when an alias does not match the slug rules.</summary>
        public static Error InvalidAlias(string alias) =>
            Error.Validation(code: InvalidAliasCode, description: $"The repository alias '{alias}' is invalid. Use lowercase letters, digits and hyphens only (max 50 characters).");

        /// <summary>Returns a conflict error when the same alias is reused inside a project.</summary>
        public static Error DuplicateAlias(RepositoryAlias alias) =>
            Error.Conflict(code: DuplicateAliasCode, description: $"A repository with alias '{alias.Value}' already exists in this project.");

        /// <summary>Returns a not-found error for the given repository identifier.</summary>
        public static Error NotFound(ProjectRepositoryId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No project repository with id '{id}' was found.");

        /// <summary>Returns a not-found error for the given repository alias.</summary>
        public static Error NotFound(RepositoryAlias alias) =>
            Error.NotFound(code: NotFoundCode, description: $"No project repository with alias '{alias.Value}' was found in this project.");

        /// <summary>Returns a conflict error when attempting to remove a repository still bound by one or more configurations.</summary>
        public static Error RepositoryInUse(string alias) =>
            Error.Conflict(code: RepositoryInUseCode, description: $"The repository '{alias}' cannot be removed because at least one infrastructure configuration is still bound to it.");

        /// <summary>Returns a validation error when no content kind is selected.</summary>
        public static Error NoContentKind() =>
            Error.Validation(code: NoContentKindCode, description: "At least one repository content kind must be selected.");

        /// <summary>Returns a validation error when the repository URL cannot be parsed.</summary>
        public static Error InvalidUrl(string url) =>
            Error.Validation(code: InvalidUrlCode, description: $"The repository URL '{url}' is invalid.");
    }
}
