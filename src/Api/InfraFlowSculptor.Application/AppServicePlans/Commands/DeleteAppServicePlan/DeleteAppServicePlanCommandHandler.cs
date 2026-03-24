using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.DeleteAppServicePlan;

/// <summary>
/// Handles the <see cref="DeleteAppServicePlanCommand"/> request.
/// Cascade-deletes all dependent Web Apps and Function Apps.
/// </summary>
public sealed class DeleteAppServicePlanCommandHandler(
    IAppServicePlanRepository appServicePlanRepository,
    IWebAppRepository webAppRepository,
    IFunctionAppRepository functionAppRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteAppServicePlanCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteAppServicePlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await appServicePlanRepository.GetByIdAsync(request.Id, cancellationToken);
        if (plan is null)
            return Errors.AppServicePlan.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(plan.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.AppServicePlan.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Cascade delete dependent Web Apps
        var dependentWebApps = await webAppRepository
            .GetByAppServicePlanIdAsync(request.Id, cancellationToken);

        foreach (var webApp in dependentWebApps)
        {
            await webAppRepository.DeleteAsync(webApp.Id);
        }

        // Cascade delete dependent Function Apps
        var dependentFunctionApps = await functionAppRepository
            .GetByAppServicePlanIdAsync(request.Id, cancellationToken);

        foreach (var functionApp in dependentFunctionApps)
        {
            await functionAppRepository.DeleteAsync(functionApp.Id);
        }

        await appServicePlanRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
