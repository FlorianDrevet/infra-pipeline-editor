using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IInfrastructureConfigRepository : IRepository<Domain.InfrastructureConfigAggregate.InfrastructureConfig>
{
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdWithMembersAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);
    Task<List<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> GetAllForUserAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the InfrastructureConfig with EnvironmentDefinitions and Members (for authorization).
    /// </summary>
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdWithEnvironmentsAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the InfrastructureConfig with ResourceNamingTemplates and Members (for authorization).
    /// </summary>
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdWithNamingTemplatesAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all infrastructure configurations belonging to the given project.
    /// </summary>
    Task<List<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> GetByProjectIdAsync(ProjectId projectId, CancellationToken cancellationToken = default);
}