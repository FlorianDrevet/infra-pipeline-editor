using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using Shared.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class KeyVaultRepository: AzureResourceRepository<KeyVault>, IKeyVaultRepository
{
    public KeyVaultRepository(ProjectDbContext context) : base(context)
    {
    }
}