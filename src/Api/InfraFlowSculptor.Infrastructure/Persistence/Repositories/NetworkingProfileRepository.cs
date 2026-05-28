using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="INetworkingProfileRepository"/>.</summary>
public sealed class NetworkingProfileRepository(ProjectDbContext context)
    : BaseRepository<NetworkingProfile, ProjectDbContext>(context), INetworkingProfileRepository
{
    /// <inheritdoc />
    public async Task<NetworkingProfile?> GetByInfraConfigIdAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        return await Context.NetworkingProfiles
            .Include(p => p.EnvironmentOverrides)
            .FirstOrDefaultAsync(p => p.InfraConfigId == infraConfigId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<NetworkingProfile?> GetByInfraConfigIdWithOverridesAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default)
    {
        return await Context.NetworkingProfiles
            .Include(p => p.EnvironmentOverrides)
            .FirstOrDefaultAsync(p => p.InfraConfigId == infraConfigId, cancellationToken);
    }
}
