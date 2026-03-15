using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IInfrastructureConfigRepository : IRepository<Domain.InfrastructureConfigAggregate.InfrastructureConfig>
{
    Task<Domain.InfrastructureConfigAggregate.InfrastructureConfig?> GetByIdWithMembersAsync(InfrastructureConfigId id, CancellationToken cancellationToken = default);
    Task<List<Domain.InfrastructureConfigAggregate.InfrastructureConfig>> GetAllForUserAsync(UserId userId, CancellationToken cancellationToken = default);
}