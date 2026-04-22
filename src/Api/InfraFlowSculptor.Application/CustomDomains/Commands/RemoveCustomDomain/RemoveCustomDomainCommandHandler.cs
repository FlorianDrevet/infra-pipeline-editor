using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.RemoveCustomDomain;

/// <summary>Handles the <see cref="RemoveCustomDomainCommand"/> request.</summary>
public sealed class RemoveCustomDomainCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<RemoveCustomDomainCommand, Deleted>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        RemoveCustomDomainCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithCustomDomainsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.CustomDomain.ResourceNotFound(request.ResourceId);

        var domain = resource.CustomDomains.FirstOrDefault(cd => cd.Id == request.CustomDomainId);
        if (domain is null)
            return Errors.CustomDomain.NotFound(request.CustomDomainId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        resource.RemoveCustomDomain(request.CustomDomainId);
        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return Result.Deleted;
    }
}
