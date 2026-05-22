using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerApps;

/// <summary>
/// Provides lightweight read projections for Container App query use cases.
/// </summary>
public interface IContainerAppReadRepository
{
    /// <summary>
    /// Gets the Container App detail projection and its owning infrastructure configuration identifier.
    /// </summary>
    /// <param name="id">The Container App identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The detail read result, or <see langword="null" /> when no Container App matches the identifier.</returns>
    Task<ContainerAppDetailReadResult?> GetByIdAsync(
        AzureResourceId id,
        CancellationToken cancellationToken = default);
}