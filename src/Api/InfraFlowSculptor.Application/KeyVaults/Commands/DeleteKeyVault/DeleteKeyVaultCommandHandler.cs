using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.DeleteKeyVault;

public class DeleteKeyVaultCommandHandler(IKeyVaultRepository keyVaultRepository)
    : IRequestHandler<DeleteKeyVaultCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteKeyVaultCommand request, CancellationToken cancellationToken)
    {
        var deleted = await keyVaultRepository.DeleteAsync(request.Id);

        if (!deleted)
            return Errors.KeyVault.NotFoundError(request.Id);

        return Result.Deleted;
    }
}
