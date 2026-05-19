using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="ProjectAggregate.Entities.ProjectRepository"/> entity.</summary>
    public static class ProjectRepository
    {
        private const string NotFoundCode = "ProjectRepository.NotFound";
        private const string NoContentKindCode = "ProjectRepository.NoContentKind";
        private const string InvalidUrlCode = "ProjectRepository.InvalidUrl";
        private const string RepositoryInUseCode = "ProjectRepository.RepositoryInUse";
        private const string PersonalAccessTokenRequiredCode = "ProjectRepository.PersonalAccessTokenRequired";
        private const string DefaultBranchNotFoundCode = "ProjectRepository.DefaultBranchNotFound";

        /// <summary>Returns a not-found error for the given repository identifier.</summary>
        public static Error NotFound(ProjectRepositoryId id) =>
            Error.NotFound(code: NotFoundCode, description: $"No project repository with id '{id}' was found.");

        /// <summary>Returns a conflict error when attempting to remove a repository still bound by one or more configurations.</summary>
        public static Error RepositoryInUse(ProjectRepositoryId repositoryId) =>
            Error.Conflict(code: RepositoryInUseCode, description: $"Repository '{repositoryId.Value}' cannot be removed because at least one infrastructure configuration is still bound to it.");

        /// <summary>Returns a validation error when no content kind is selected.</summary>
        public static Error NoContentKind() =>
            Error.Validation(code: NoContentKindCode, description: "At least one repository content kind must be selected.");

        /// <summary>Returns a validation error when the repository URL cannot be parsed.</summary>
        public static Error InvalidUrl(string url) =>
            Error.Validation(code: InvalidUrlCode, description: $"The repository URL '{url}' is invalid.");

        /// <summary>Returns a validation error when a configured repository create/update needs a PAT.</summary>
        public static Error PersonalAccessTokenRequired() =>
            Error.Validation(code: PersonalAccessTokenRequiredCode, description: "A personal access token is required to verify this repository configuration.");

        /// <summary>Returns a validation error when the configured default branch is missing remotely.</summary>
        public static Error DefaultBranchNotFound(string defaultBranch) =>
            Error.Validation(code: DefaultBranchNotFoundCode, description: $"The default branch '{defaultBranch}' was not found in the repository.");
    }
}
