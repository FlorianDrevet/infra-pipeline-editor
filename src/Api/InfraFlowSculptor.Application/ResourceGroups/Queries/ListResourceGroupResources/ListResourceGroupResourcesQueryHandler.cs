using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupResources;

public class ListResourceGroupResourcesQueryHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IQueryHandler<ListResourceGroupResourcesQuery, List<AzureResourceResult>>
{
    public async Task<ErrorOr<List<AzureResourceResult>>> Handle(
        ListResourceGroupResourcesQuery query, CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdWithResourcesAsync(query.Id, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return Errors.ResourceGroup.NotFound(query.Id);

        // Query parent FK mappings directly from child TPT tables
        // to guarantee correct resolution regardless of TPT materialization order.
        var parentMapping = await resourceGroupRepository.GetChildToParentMappingAsync(query.Id, cancellationToken);

        // Query configured environment names per resource from all TPT environment settings tables.
        var envMapping = await resourceGroupRepository.GetConfiguredEnvironmentsByResourceGroupAsync(query.Id, cancellationToken);

        return resourceGroup.Resources
            .Select(r =>
            {
                var parentId = parentMapping.TryGetValue(r.Id.Value, out var pid)
                    ? new AzureResourceId(pid)
                    : null;
                var configuredEnvs = envMapping.TryGetValue(r.Id.Value, out var envs)
                    ? (IReadOnlyCollection<string>)envs
                    : Array.Empty<string>();
                return new AzureResourceResult(r.Id, r.GetType().Name, r.Name, r.Location, parentId, configuredEnvs);
            })
            .ToList();
    }
}
