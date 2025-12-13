using InfraFlowSculptor.Domain.UserAggregate;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IUserRepository: IRepository<User>
{
    Task<User> GetOrCreateByEntraIdAsync(string entraId, string firstName, string lastName, CancellationToken cancellationToken = default);
}