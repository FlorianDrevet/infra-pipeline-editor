using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupResources;

public class ListResourceGroupResourcesQueryHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfrastructureConfigRepository infraConfigRepository,
    ICurrentUser currentUser)
    : IRequestHandler<ListResourceGroupResourcesQuery, ErrorOr<List<AzureResourceResult>>>
{
    public async Task<ErrorOr<List<AzureResourceResult>>> Handle(
        ListResourceGroupResourcesQuery query, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdWithResourcesAsync(query.Id, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(query.Id);

        var authResult = await InfraConfigAccessHelper.VerifyReadAccessAsync(
            infraConfigRepository, currentUser, resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.ResourceGroup.NotFound(query.Id);

        return resourceGroup.Resources
            .Select(r => new AzureResourceResult(r.Id, r.GetType().Name, r.Name, r.Location))
            .ToList();
    }
}
