using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core repository for <see cref="UserAssignedIdentity"/> aggregates.
/// </summary>
public sealed class UserAssignedIdentityRepository
    : AzureResourceRepository<UserAssignedIdentity>, IUserAssignedIdentityRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserAssignedIdentityRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserAssignedIdentityRepository(ProjectDbContext context) : base(context) { }

    /// <inheritdoc />
    public override async Task<UserAssignedIdentity?> GetByIdAsync(
        ValueObject id,
        CancellationToken cancellationToken)
    {
        return await Context.Set<UserAssignedIdentity>()
            .Include(uai => uai.DependsOn)
            .FirstOrDefaultAsync(uai => uai.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<UserAssignedIdentity>> GetByResourceGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<UserAssignedIdentity>()
            .Where(uai => uai.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
