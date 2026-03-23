using ErrorOr;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.UpdateAppServicePlan;

/// <summary>Handles the <see cref="UpdateAppServicePlanCommand"/> request.</summary>
public class UpdateAppServicePlanCommandHandler(
    IAppServicePlanRepository appServicePlanRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<UpdateAppServicePlanCommand, ErrorOr<AppServicePlanResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AppServicePlanResult>> Handle(
        UpdateAppServicePlanCommand request,
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

        var osType = new AppServicePlanOsType(
            Enum.Parse<AppServicePlanOsType.AppServicePlanOsTypeEnum>(request.OsType));

        plan.Update(request.Name, request.Location, osType);

        if (request.EnvironmentSettings is not null)
            plan.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName,
                        ec.Sku is not null
                            ? new AppServicePlanSku(Enum.Parse<AppServicePlanSku.AppServicePlanSkuEnum>(ec.Sku))
                            : (AppServicePlanSku?)null,
                        ec.Capacity))
                    .ToList());

        var updated = await appServicePlanRepository.UpdateAsync(plan);

        return mapper.Map<AppServicePlanResult>(updated);
    }
}
