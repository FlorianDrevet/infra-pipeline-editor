using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.DeleteAppServicePlan;

/// <summary>Handles the <see cref="DeleteAppServicePlanCommand"/> request.</summary>
public class DeleteAppServicePlanCommandHandler(
    IAppServicePlanRepository appServicePlanRepository,
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

        await appServicePlanRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
