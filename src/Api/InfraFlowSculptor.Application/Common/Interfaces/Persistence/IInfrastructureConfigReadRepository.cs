using InfraFlowSculptor.Application.InfrastructureConfig.ReadModels;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IInfrastructureConfigReadRepository
{
    Task<InfrastructureConfigReadModel?> GetByIdWithResourcesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all infrastructure configurations for a project, each fully loaded
    /// with resources, role assignments, app settings, and cross-config references.
    /// Used for mono-repo Bicep generation.
    /// </summary>
    Task<IReadOnlyCollection<InfrastructureConfigReadModel>> GetAllByProjectIdWithResourcesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);
}
