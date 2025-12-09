using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.KeyVaultAggregate;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class KeyVaultRepository: BaseRepository<KeyVault, ProjectDbContext>, IKeyVaultRepository
{
    public KeyVaultRepository(ProjectDbContext context) : base(context)
    {
    }
}