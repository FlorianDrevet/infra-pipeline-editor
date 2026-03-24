using ErrorOr;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.AppServicePlans.Queries;

/// <summary>Handles the <see cref="GetAppServicePlanQuery"/> request.</summary>
public class GetAppServicePlanQueryHandler(
    IAppServicePlanRepository appServicePlanRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<GetAppServicePlanQuery, ErrorOr<AppServicePlanResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AppServicePlanResult>> Handle(
        GetAppServicePlanQuery query,
        CancellationToken cancellationToken)
    {
        var plan = await appServicePlanRepository.GetByIdAsync(query.Id, cancellationToken);
        if (plan is null)
            return Errors.AppServicePlan.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(plan.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.AppServicePlan.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.AppServicePlan.NotFoundError(query.Id);

        return mapper.Map<AppServicePlanResult>(plan);
    }
}
