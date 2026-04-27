using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IUserRepository"/>.</summary>
public sealed class UserRepository(ProjectDbContext context)
    : BaseRepository<User, ProjectDbContext>(context), IUserRepository
{
    /// <inheritdoc />
    public async Task<User?> GetByEntraIdAsync(string entraId, CancellationToken cancellationToken = default)
    {
        var entreId = new EntraId(Guid.Parse(entraId));
        return await Context.Users
            .FirstOrDefaultAsync(u => u.EntraId == entreId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<User>> GetByIdsAsync(IReadOnlyList<UserId> ids, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}