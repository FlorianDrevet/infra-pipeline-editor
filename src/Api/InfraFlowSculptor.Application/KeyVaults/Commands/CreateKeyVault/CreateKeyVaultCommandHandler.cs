using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

public class CreateKeyVaultCommandHandler(IKeyVaultRepository keyVaultRepository, IMapper mapper) : IRequestHandler<CreateKeyVaultCommand, ErrorOr<KeyVaultResult>>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(CreateKeyVaultCommand request, CancellationToken cancellationToken)
    {
        var keyVault = KeyVault.Create(request.ResourceGroupId, request.Name, request.Location, request.Sku);

        var savedKeyVault =  await keyVaultRepository.AddAsync(keyVault);

        return mapper.Map<KeyVaultResult>(savedKeyVault);
    }
}