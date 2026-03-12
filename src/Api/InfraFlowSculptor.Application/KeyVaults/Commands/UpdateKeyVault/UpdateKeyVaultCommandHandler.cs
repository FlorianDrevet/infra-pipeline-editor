using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;

public class UpdateKeyVaultCommandHandler(IKeyVaultRepository keyVaultRepository, IMapper mapper)
    : IRequestHandler<UpdateKeyVaultCommand, ErrorOr<KeyVaultResult>>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(UpdateKeyVaultCommand request, CancellationToken cancellationToken)
    {
        var keyVault = await keyVaultRepository.GetByIdAsync(request.Id, cancellationToken);

        if (keyVault is null)
            return Errors.KeyVault.NotFoundError(request.Id);

        keyVault.Update(request.Name, request.Location, request.Sku);

        var updatedKeyVault = await keyVaultRepository.UpdateAsync(keyVault);

        return mapper.Map<KeyVaultResult>(updatedKeyVault);
    }
}
