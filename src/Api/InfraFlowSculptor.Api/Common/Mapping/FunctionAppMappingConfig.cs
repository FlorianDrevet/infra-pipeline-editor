using InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;
using InfraFlowSculptor.Application.FunctionApps.Commands.UpdateFunctionApp;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Contracts.FunctionApps.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FunctionAppAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Function App feature.</summary>
public sealed class FunctionAppMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateFunctionAppRequest, CreateFunctionAppCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new FunctionAppEnvironmentConfigData(
                        ec.EnvironmentName, ec.HttpsOnly, ec.RuntimeStack, ec.RuntimeVersion, ec.MaxInstanceCount, ec.FunctionsWorkerRuntime)).ToList());

        config.NewConfig<(Guid Id, UpdateFunctionAppRequest Request), UpdateFunctionAppCommand>()
            .MapWith(src => new UpdateFunctionAppCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.AppServicePlanId,
                src.Request.RuntimeStack,
                src.Request.RuntimeVersion,
                src.Request.HttpsOnly,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new FunctionAppEnvironmentConfigData(
                        ec.EnvironmentName, ec.HttpsOnly, ec.RuntimeStack, ec.RuntimeVersion, ec.MaxInstanceCount, ec.FunctionsWorkerRuntime)).ToList()));

        config.NewConfig<FunctionApp, FunctionAppResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new FunctionAppEnvironmentConfigData(
                    es.EnvironmentName,
                    es.HttpsOnly,
                    (object?)es.RuntimeStack != null ? es.RuntimeStack.Value.ToString() : null,
                    es.RuntimeVersion,
                    es.MaxInstanceCount,
                    es.FunctionsWorkerRuntime)).ToList())
            .Map(dest => dest.RuntimeStack, src => src.RuntimeStack.Value.ToString())
            .Map(dest => dest.AppServicePlanId, src => src.AppServicePlanId.Value);

        config.NewConfig<FunctionAppEnvironmentConfigData, FunctionAppEnvironmentConfigResponse>()
            .MapWith(src => new FunctionAppEnvironmentConfigResponse(
                src.EnvironmentName, src.HttpsOnly, src.RuntimeStack, src.RuntimeVersion, src.MaxInstanceCount, src.FunctionsWorkerRuntime));
    }
}
