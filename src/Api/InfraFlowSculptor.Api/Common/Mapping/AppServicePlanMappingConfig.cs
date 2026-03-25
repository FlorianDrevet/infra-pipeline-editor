using InfraFlowSculptor.Application.AppServicePlans.Commands.CreateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Commands.UpdateAppServicePlan;
using InfraFlowSculptor.Application.AppServicePlans.Common;
using InfraFlowSculptor.Contracts.AppServicePlans.Requests;
using InfraFlowSculptor.Domain.AppServicePlanAggregate;
using InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the App Service Plan feature.</summary>
public sealed class AppServicePlanMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateAppServicePlanRequest, CreateAppServicePlanCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new AppServicePlanEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku, ec.Capacity)).ToList());

        config.NewConfig<(Guid Id, UpdateAppServicePlanRequest Request), UpdateAppServicePlanCommand>()
            .MapWith(src => new UpdateAppServicePlanCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.OsType,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new AppServicePlanEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku, ec.Capacity)).ToList()));

        config.NewConfig<AppServicePlan, AppServicePlanResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new AppServicePlanEnvironmentConfigData(
                    es.EnvironmentName,
                    (object?)es.Sku != null ? es.Sku.Value.ToString() : null,
                    es.Capacity)).ToList())
            .Map(dest => dest.OsType, src => src.OsType.Value.ToString());

        config.NewConfig<AppServicePlanEnvironmentConfigData, AppServicePlanEnvironmentConfigResponse>()
            .MapWith(src => new AppServicePlanEnvironmentConfigResponse(
                src.EnvironmentName, src.Sku, src.Capacity));
    }
}
