using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.GenerationCore;
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
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(query.Id, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        // Query parent FK mappings directly from child TPT tables
        // to guarantee correct resolution regardless of TPT materialization order.
        var parentMapping = await resourceGroupRepository.GetChildToParentMappingAsync(query.Id, cancellationToken);

        // Query configured environment names from the vw_ResourceEnvironmentEntries view.
        var envMapping = await resourceGroupRepository.GetConfiguredEnvironmentsByResourceGroupAsync(query.Id, cancellationToken);

        // Lightweight projection from AzureResource base table — no TPT JOINs.
        var summaries = await resourceGroupRepository.GetResourceSummariesByGroupIdAsync(query.Id, cancellationToken);

        var storageAccountIds = summaries
            .Where(resource => resource.ResourceType == AzureResourceTypes.StorageAccount)
            .Select(resource => new AzureResourceId(resource.Id))
            .ToList();

        var storageSubResourcesByStorageId = await resourceGroupRepository.GetStorageSubResourcesByStorageAccountIdsAsync(
            storageAccountIds,
            cancellationToken);

        return summaries
            .Select(r =>
            {
                var parentId = parentMapping.TryGetValue(r.Id, out var pid)
                    ? new AzureResourceId(pid)
                    : null;
                var configuredEnvs = envMapping.TryGetValue(r.Id, out var envs)
                    ? (IReadOnlyList<string>)envs
                    : Array.Empty<string>();
                var storageSubResources = storageSubResourcesByStorageId.TryGetValue(r.Id, out var storageChildren)
                    ? storageChildren
                    : null;
                return new AzureResourceResult(
                    new AzureResourceId(r.Id),
                    r.ResourceType,
                    new Name(r.Name),
                    new Location(Enum.Parse<Location.LocationEnum>(r.Location)),
                    parentId,
                    configuredEnvs,
                    r.IsExisting,
                    storageSubResources);
            })
            .ToList();
    }
}
