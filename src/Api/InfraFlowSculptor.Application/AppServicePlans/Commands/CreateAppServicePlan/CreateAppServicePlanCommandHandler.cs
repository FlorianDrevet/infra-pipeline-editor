using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;

/// <summary>Handles the <see cref="CreateAppServicePlanCommand"/> request.</summary>
public class CreateAppServicePlanCommandHandler(
    IAppServicePlanRepository appServicePlanRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateAppServicePlanCommand, AppServicePlanResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<AppServicePlanResult>> Handle(
        CreateAppServicePlanCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var osType = new AppServicePlanOsType(
            Enum.Parse<AppServicePlanOsType.AppServicePlanOsTypeEnum>(request.OsType));

        var plan = AppServicePlan.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            osType,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName,
                    ec.Sku is not null
                        ? new AppServicePlanSku(Enum.Parse<AppServicePlanSku.AppServicePlanSkuEnum>(ec.Sku))
                        : (AppServicePlanSku?)null,
                    ec.Capacity))
                .ToList(),
            isExisting: request.IsExisting);

        var saved = await appServicePlanRepository.AddAsync(plan);

        return mapper.Map<AppServicePlanResult>(saved);
    }
}
