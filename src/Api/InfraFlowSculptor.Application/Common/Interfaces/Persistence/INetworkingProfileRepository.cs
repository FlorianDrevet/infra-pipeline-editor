using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

/// <summary>Repository for <see cref="NetworkingProfile"/> aggregates.</summary>
public interface INetworkingProfileRepository : IRepository<NetworkingProfile>
{
    /// <summary>
    /// Returns the networking profile for the given infrastructure configuration, or <c>null</c> if none exists.
    /// </summary>
    Task<NetworkingProfile?> GetByInfraConfigIdAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the networking profile with environment overrides loaded for the given infra config.
    /// </summary>
    Task<NetworkingProfile?> GetByInfraConfigIdWithOverridesAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default);
}
