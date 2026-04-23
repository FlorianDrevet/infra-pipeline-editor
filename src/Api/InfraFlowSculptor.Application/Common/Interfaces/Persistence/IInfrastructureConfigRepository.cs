using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IInfrastructureConfigRepository : IRepository<Domain.InfrastructureConfigAggregate.InfrastructureConfig>
{
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdWithMembersAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);
    Task<List<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> GetAllForUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the InfrastructureConfig with ResourceNamingTemplates and Members (for authorization).
    /// </summary>
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdWithNamingTemplatesAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all infrastructure configurations belonging to the given project.
    /// </summary>
    Task<List<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns lightweight summaries (Id, Name) for configurations the given user has access to.
    /// Use this projection when only summary fields are needed, avoiding loading full aggregates.
    /// </summary>
    Task<List<InfraConfigSummary>> GetConfigSummariesForUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if at least one configuration of the given project is bound to the
    /// project-level repository identified by <paramref name="alias"/>.
    /// Used to forbid deletion of a <c>ProjectRepository</c> that is still referenced.
    /// </summary>
    /// <param name="projectId">The parent project identifier.</param>
    /// <param name="alias">The repository alias to test.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when at least one binding exists; otherwise <c>false</c>.</returns>
    Task<bool> AnyBoundToRepositoryAliasAsync(
        ProjectId projectId,
        RepositoryAlias alias,
        CancellationToken cancellationToken = default);
}