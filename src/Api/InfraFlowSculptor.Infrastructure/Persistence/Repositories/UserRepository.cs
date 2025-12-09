using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class UserRepository : BaseRepository<User, ProjectDbContext>, IUserRepository
{
    public UserRepository(ProjectDbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await Context.Users
            .FirstOrDefaultAsync(user => user.Email == email);
    }
}