using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IUserRepository"/>.</summary>
public class UserRepository : BaseRepository<User, ProjectDbContext>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserRepository(ProjectDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<User> GetOrCreateByEntraIdAsync(string entraId, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var entreId = new EntraId(Guid.Parse(entraId));
        var user = await Context.Users.FirstOrDefaultAsync(u => u.EntraId == entreId, cancellationToken: cancellationToken);

        if (user is not null)
            return user;

        user = User.Create(entreId, new Name(firstName, lastName));

        await Context.Users.AddAsync(user, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return user;
    }

    /// <inheritdoc />
    public async Task<List<User>> GetByIdsAsync(IReadOnlyList<UserId> ids, CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}