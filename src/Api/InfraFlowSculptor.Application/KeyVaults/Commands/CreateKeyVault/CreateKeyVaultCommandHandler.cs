using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

public class CreateKeyVaultCommandHandler : IRequestHandler<CreateKeyVaultCommand, CreateKeyVaultResult>
{

    public CreateKeyVaultCommandHandler()
    {
    }

    public async Task<CreateKeyVaultResult> Handle(CreateKeyVaultCommand request, CancellationToken cancellationToken)
    {
        return null;
    }
}