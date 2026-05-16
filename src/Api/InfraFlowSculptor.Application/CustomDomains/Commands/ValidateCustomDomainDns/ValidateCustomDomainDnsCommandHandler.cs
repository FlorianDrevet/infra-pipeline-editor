using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.ValidateCustomDomainDns;

/// <summary>Handles the <see cref="ValidateCustomDomainDnsCommand"/> request.</summary>
public sealed class ValidateCustomDomainDnsCommandHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : ICommandHandler<ValidateCustomDomainDnsCommand, CustomDomainResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<CustomDomainResult>> Handle(
        ValidateCustomDomainDnsCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await azureResourceRepository.GetByIdWithCustomDomainsAsync(
            request.ResourceId, cancellationToken);

        if (resource is null)
            return Errors.CustomDomain.ResourceNotFound(request.ResourceId);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(
            resource.ResourceGroupId, cancellationToken);

        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(resource.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var domain = resource.CustomDomains.FirstOrDefault(cd => cd.Id == request.CustomDomainId);

        if (domain is null)
            return Errors.CustomDomain.NotFound(request.CustomDomainId);

        domain.ValidateDns();

        await azureResourceRepository.UpdateAsync(resource, cancellationToken);

        return new CustomDomainResult(
            domain.Id,
            domain.ResourceId,
            domain.EnvironmentName,
            domain.DomainName,
            domain.CertificateMode.Value.ToString(),
            domain.KeyVaultUrl,
            domain.ManagedIdentityResourceId,
            domain.CertificateName,
            domain.DnsValidationStatus.Value.ToString());
    }
}
