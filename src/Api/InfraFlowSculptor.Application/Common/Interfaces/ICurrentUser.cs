using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Common.Interfaces;

public interface ICurrentUser
{
    Task<UserId> GetUserIdAsync(CancellationToken cancellationToken = default);
}