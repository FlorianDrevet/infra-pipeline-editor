using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IInfrastructureConfigRepository : IRepository<Domain.InfrastructureConfigAggregate.InfrastructureConfig>
{
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdReadOnlyAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdWithMembersAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdWithMembersReadOnlyAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);
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
    /// Returns lightweight summaries (Id, Name) for the specified infrastructure configuration identifiers.
    /// Use this projection when resolving multiple target configurations without loading full aggregates.
    /// </summary>
    Task<List<InfraConfigSummary>> GetConfigSummariesByIdsAsync(
        IReadOnlyList<InfrastructureConfigId> ids,
        CancellationToken cancellationToken = default);
}