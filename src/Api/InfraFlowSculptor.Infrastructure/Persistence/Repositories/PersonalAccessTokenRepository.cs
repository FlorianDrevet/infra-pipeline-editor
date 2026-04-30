using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IPersonalAccessTokenRepository"/>.</summary>
public sealed class PersonalAccessTokenRepository(ProjectDbContext context)
    : BaseRepository<PersonalAccessToken, ProjectDbContext>(context), IPersonalAccessTokenRepository
{
    /// <inheritdoc />
    public async Task<PersonalAccessToken?> GetByTokenHashAsync(
        TokenHash tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await Context.PersonalAccessTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PersonalAccessToken>> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.PersonalAccessTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
