using BicepGenerator.Application.InfrastructureConfig.ReadModels;

namespace BicepGenerator.Application.Common.Interfaces.Persistence;

public interface IInfrastructureConfigReadRepository
{
    Task<InfrastructureConfigReadModel?> GetByIdWithResourcesAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
