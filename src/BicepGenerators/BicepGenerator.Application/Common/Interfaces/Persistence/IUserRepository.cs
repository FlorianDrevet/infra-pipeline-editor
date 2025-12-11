using BicepGenerator.Domain.UserAggregate;
using BicepGenerator.Domain.UserAggregate.ValueObjects;

namespace BicepGenerator.Application.Common.Interfaces.Persistence;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
}