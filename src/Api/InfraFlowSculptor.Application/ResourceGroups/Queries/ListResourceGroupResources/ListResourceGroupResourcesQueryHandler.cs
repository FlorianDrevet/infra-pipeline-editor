using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using InfraFlowSculptor.Application.ResourceGroups.Common;
using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ContainerAppAggregate;
using ApplicationInsightsEntity = InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ApplicationInsights;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.WebAppAggregate;
using MediatR;

namespace InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupResources;

public class ListResourceGroupResourcesQueryHandler(
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<ListResourceGroupResourcesQuery, ErrorOr<List<AzureResourceResult>>>
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

        return resourceGroup.Resources
            .Select(r => new AzureResourceResult(r.Id, r.GetType().Name, r.Name, r.Location, ResolveParentResourceId(r)))
            .ToList();
    }

    /// <summary>Extracts the parent resource identifier from child resource types.</summary>
    private static AzureResourceId? ResolveParentResourceId(AzureResource resource) => resource switch
    {
        WebApp wa => wa.AppServicePlanId,
        FunctionApp fa => fa.AppServicePlanId,
        ContainerApp ca => ca.ContainerAppEnvironmentId,
        SqlDatabase sqlDb => sqlDb.SqlServerId,
        ApplicationInsightsEntity ai => ai.LogAnalyticsWorkspaceId,
        _ => null,
    };
}
