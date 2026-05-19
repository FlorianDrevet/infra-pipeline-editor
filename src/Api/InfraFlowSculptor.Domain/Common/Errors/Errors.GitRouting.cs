using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Errors raised by the V2 multi-repo Git routing layer (<c>IRepositoryTargetResolver</c>).</summary>
    public static class GitRouting
    {
        private const string NoRepositoryConfiguredCode = "GitRouting.NoRepositoryConfigured";
        private const string RepositorySlotNotConfiguredCode = "GitRouting.RepositorySlotNotConfigured";
        private const string RepositoryRoleNotConfiguredCode = "GitRouting.RepositoryRoleNotConfigured";
        private const string RepositoryRoleMismatchCode = "GitRouting.RepositoryRoleMismatch";
        private const string AmbiguousProjectLevelGenerationCode = "GitRouting.AmbiguousProjectLevelGeneration";
        private const string LayoutNotSupportedForMultiRepoPushCode = "GitRouting.LayoutNotSupportedForMultiRepoPush";

        /// <summary>
        /// Returned when no repository can be resolved for a project: neither a project-level
        /// <c>ProjectRepository</c> nor a legacy <c>GitRepositoryConfiguration</c> is configured.
        /// </summary>
        /// <param name="projectId">The identifier of the project missing a repository configuration.</param>
        public static Error NoRepositoryConfigured(ProjectId projectId) =>
            Error.Validation(
                code: NoRepositoryConfiguredCode,
                description: $"Project '{projectId.Value}' has no repository configured.");

        /// <summary>
        /// Returned when a repository slot was created but its connection
        /// details (provider, URL, default branch) have not been provided yet.
        /// </summary>
        /// <param name="repositoryId">The unconfigured repository slot identifier.</param>
        public static Error RepositorySlotNotConfigured(ProjectRepositoryId repositoryId) =>
            Error.Validation(
                code: RepositorySlotNotConfiguredCode,
                description: $"Repository '{repositoryId.Value}' is declared but its Git connection details (provider, URL, default branch) are not configured yet.");

        /// <summary>Returned when no repository is configured for the requested content kind.</summary>
        public static Error RepositoryRoleNotConfigured(RepositoryContentKindsEnum contentKind) =>
            Error.NotFound(
                code: RepositoryRoleNotConfiguredCode,
                description: $"No repository is configured for content kind '{contentKind}'.");

        /// <summary>Returned when a requested repository id does not match the repository for the requested content kind.</summary>
        public static Error RepositoryRoleMismatch(ProjectRepositoryId repositoryId, RepositoryContentKindsEnum contentKind) =>
            Error.Validation(
                code: RepositoryRoleMismatchCode,
                description: $"Repository '{repositoryId.Value}' does not host content kind '{contentKind}'.");

        /// <summary>
        /// Returned when a project-level "generate-all" operation is attempted while the project
        /// declares a heterogeneous multi-repo topology (configurations bound to different repositories).
        /// </summary>
        public static Error AmbiguousProjectLevelGeneration =>
            Error.Validation(
                code: AmbiguousProjectLevelGenerationCode,
                description: "Project-level generate-all is disabled for heterogeneous multi-repo topologies.");

        /// <summary>
        /// Returned when a SplitInfraCode dual-push is requested on a project whose layout preset
        /// does not match (AllInOne or MultiRepo). Consumers must use the layout-specific push
        /// endpoints instead.
        /// </summary>
        public static Error LayoutNotSupportedForMultiRepoPush =>
            Error.Validation(
                code: LayoutNotSupportedForMultiRepoPushCode,
                description: "Multi-repo dual push is only supported for projects with the SplitInfraCode layout preset.");
    }
}
