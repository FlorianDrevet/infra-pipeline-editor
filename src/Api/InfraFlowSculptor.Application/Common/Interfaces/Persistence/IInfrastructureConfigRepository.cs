using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IInfrastructureConfigRepository : IRepository<Domain.InfrastructureConfigAggregate.InfrastructureConfig>
{
}