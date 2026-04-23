using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Errors raised by the V2 multi-repo Git routing layer (<c>IRepositoryTargetResolver</c>).</summary>
    public static class GitRouting
    {
        private const string AliasNotFoundCode = "GitRouting.AliasNotFound";
        private const string NoRepositoryConfiguredCode = "GitRouting.NoRepositoryConfigured";
        private const string AmbiguousProjectLevelGenerationCode = "GitRouting.AmbiguousProjectLevelGeneration";

        /// <summary>
        /// Returned when an <c>InfrastructureConfig.RepositoryBinding</c> or a fallback alias
        /// points to an alias that is not declared on the project.
        /// </summary>
        /// <param name="alias">The alias that could not be resolved.</param>
        public static Error AliasNotFound(string alias) =>
            Error.NotFound(
                code: AliasNotFoundCode,
                description: $"Repository alias '{alias}' is not declared on the project.");

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
        /// Returned when a project-level "generate-all" operation is attempted while the project
        /// declares a heterogeneous multi-repo topology (configurations bound to different repositories).
        /// </summary>
        public static Error AmbiguousProjectLevelGeneration =>
            Error.Validation(
                code: AmbiguousProjectLevelGenerationCode,
                description: "Project-level generate-all is disabled for heterogeneous multi-repo topologies.");
    }
}
