using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels;
using MediatR;

namespace InfraFlowSculptor.Application.Common.Queries.GetDependentResources;

/// <summary>
/// Handles the <see cref="GetDependentResourcesQuery"/> by looking up all child resources
/// that depend on the given parent resource (LogAnalyticsWorkspace, AppServicePlan, SqlServer).
/// </summary>
public sealed class GetDependentResourcesQueryHandler(
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
    IAppServicePlanRepository appServicePlanRepository,
    ISqlServerRepository sqlServerRepository,
    IApplicationInsightsRepository applicationInsightsRepository,
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository,
    ISqlDatabaseRepository sqlDatabaseRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<GetDependentResourcesQuery, ErrorOr<List<DependentResourceResult>>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<DependentResourceResult>>> Handle(
        GetDependentResourcesQuery request,
        CancellationToken cancellationToken)
    {
        // Try to find the parent resource and determine its type
        AzureResource? parent = await logAnalyticsWorkspaceRepository.GetByIdAsync(request.Id, cancellationToken);
        var parentType = "LogAnalyticsWorkspace";

        if (parent is null)
        {
            parent = await appServicePlanRepository.GetByIdAsync(request.Id, cancellationToken);
            parentType = "AppServicePlan";
        }

        if (parent is null)
        {
            parent = await sqlServerRepository.GetByIdAsync(request.Id, cancellationToken);
            parentType = "SqlServer";
        }

        if (parent is null)
            return new List<DependentResourceResult>();

        // Verify read access
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(parent.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return new List<DependentResourceResult>();

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Query dependents based on parent type
        var dependents = new List<DependentResourceResult>();

        switch (parentType)
        {
            case "LogAnalyticsWorkspace":
            {
                var appInsights = await applicationInsightsRepository
                    .GetByLogAnalyticsWorkspaceIdAsync(request.Id, cancellationToken);
                dependents.AddRange(appInsights.Select(ai => new DependentResourceResult(
                    ai.Id.Value, ai.Name.Value, "ApplicationInsights")));
                break;
            }
            case "AppServicePlan":
            {
                var webApps = await webAppRepository
                    .GetByAppServicePlanIdAsync(request.Id, cancellationToken);
                dependents.AddRange(webApps.Select(wa => new DependentResourceResult(
                    wa.Id.Value, wa.Name.Value, "WebApp")));

                var functionApps = await functionAppRepository
                    .GetByAppServicePlanIdAsync(request.Id, cancellationToken);
                dependents.AddRange(functionApps.Select(fa => new DependentResourceResult(
                    fa.Id.Value, fa.Name.Value, "FunctionApp")));
                break;
            }
            case "SqlServer":
            {
                var sqlDatabases = await sqlDatabaseRepository
                    .GetBySqlServerIdAsync(request.Id, cancellationToken);
                dependents.AddRange(sqlDatabases.Select(db => new DependentResourceResult(
                    db.Id.Value, db.Name.Value, "SqlDatabase")));
                break;
            }
        }

        return dependents;
    }
}
