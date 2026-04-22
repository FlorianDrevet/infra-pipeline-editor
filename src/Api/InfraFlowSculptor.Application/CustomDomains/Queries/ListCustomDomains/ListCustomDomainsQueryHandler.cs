using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Domain.Common.Errors;

namespace InfraFlowSculptor.Application.CustomDomains.Queries.ListCustomDomains;

/// <summary>Handles the <see cref="ListCustomDomainsQuery"/> request.</summary>
public sealed class ListCustomDomainsQueryHandler(
    IAzureResourceRepository azureResourceRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IQueryHandler<ListCustomDomainsQuery, IReadOnlyList<CustomDomainResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<IReadOnlyList<CustomDomainResult>>> Handle(
        ListCustomDomainsQuery request,
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

        var authResult = await accessService.VerifyReadAccessAsync(
            resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        IReadOnlyList<CustomDomainResult> results = resource.CustomDomains
            .Select(cd => new CustomDomainResult(
                cd.Id,
                cd.ResourceId,
                cd.EnvironmentName,
                cd.DomainName,
                cd.BindingType))
            .ToList();

        return results.ToList();
    }
}
