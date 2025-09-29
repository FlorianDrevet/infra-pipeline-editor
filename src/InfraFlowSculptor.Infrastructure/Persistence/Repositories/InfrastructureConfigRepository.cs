using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Infrastructure.Persistence;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class InfrastructureConfigRepository : BaseRepository<InfrastructureConfig, ProjectDbContext>, IInfrastructureConfigRepository
{
    public InfrastructureConfigRepository(ProjectDbContext context) : base(context)
    {
    }
}