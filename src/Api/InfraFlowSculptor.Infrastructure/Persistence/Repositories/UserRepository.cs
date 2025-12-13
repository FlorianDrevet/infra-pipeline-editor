using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class UserRepository: BaseRepository<User, ProjectDbContext>, IUserRepository
{
    public UserRepository(ProjectDbContext context) : base(context)
    {
    }

    public async Task<User> GetOrCreateByEntraIdAsync(string entraId, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var entreId = new EntraId(Guid.Parse(entraId));
        var user = await Context.Users.FirstOrDefaultAsync(u => u.EntraId == entreId, cancellationToken: cancellationToken);

        if (user is not null)
            return user;

        user = User.Create(entreId, new Name(firstName, lastName));

        await Context.Users.AddAsync(user, cancellationToken);

        return user;
    }
}