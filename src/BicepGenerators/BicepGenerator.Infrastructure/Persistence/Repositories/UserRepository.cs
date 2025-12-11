using Microsoft.EntityFrameworkCore;
using BicepGenerator.Application.Common.Interfaces.Persistence;
using BicepGenerator.Domain.UserAggregate;
using BicepGenerator.Domain.UserAggregate.ValueObjects;

namespace BicepGenerator.Infrastructure.Persistence.Repositories;

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