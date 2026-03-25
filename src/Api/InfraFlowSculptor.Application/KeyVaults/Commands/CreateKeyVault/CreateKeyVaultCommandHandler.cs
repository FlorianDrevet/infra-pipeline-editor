using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;

public class CreateKeyVaultCommandHandler(
    IKeyVaultRepository keyVaultRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<CreateKeyVaultCommand, ErrorOr<KeyVaultResult>>
{
    public async Task<ErrorOr<KeyVaultResult>> Handle(CreateKeyVaultCommand request, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var keyVault = KeyVault.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnableRbacAuthorization,
            request.EnabledForDeployment,
            request.EnabledForDiskEncryption,
            request.EnabledForTemplateDeployment,
            request.EnablePurgeProtection,
            request.EnableSoftDelete,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName,
                    ec.Sku is not null ? new Sku(Enum.Parse<Sku.SkuEnum>(ec.Sku)) : (Sku?)null))
                .ToList());

        var savedKeyVault = await keyVaultRepository.AddAsync(keyVault);

        return mapper.Map<KeyVaultResult>(savedKeyVault);
    }
}
