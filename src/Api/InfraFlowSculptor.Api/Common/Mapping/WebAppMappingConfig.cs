using InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;
using InfraFlowSculptor.Application.WebApps.Commands.UpdateWebApp;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Contracts.WebApps.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.WebAppAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Web App feature.</summary>
public sealed class WebAppMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateWebAppRequest, CreateWebAppCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new WebAppEnvironmentConfigData(
                        ec.EnvironmentName, ec.AlwaysOn, ec.HttpsOnly, ec.RuntimeStack, ec.RuntimeVersion, ec.DockerImageTag)).ToList());

        config.NewConfig<(Guid Id, UpdateWebAppRequest Request), UpdateWebAppCommand>()
            .MapWith(src => new UpdateWebAppCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.AppServicePlanId,
                src.Request.RuntimeStack,
                src.Request.RuntimeVersion,
                src.Request.AlwaysOn,
                src.Request.HttpsOnly,
                src.Request.DeploymentMode,
                src.Request.ContainerRegistryId,
                src.Request.DockerImageName,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new WebAppEnvironmentConfigData(
                        ec.EnvironmentName, ec.AlwaysOn, ec.HttpsOnly, ec.RuntimeStack, ec.RuntimeVersion, ec.DockerImageTag)).ToList()));

        config.NewConfig<WebApp, WebAppResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new WebAppEnvironmentConfigData(
                    es.EnvironmentName,
                    es.AlwaysOn,
                    es.HttpsOnly,
                    es.RuntimeStack != null ? es.RuntimeStack.Value.ToString() : null,
                    es.RuntimeVersion,
                    es.DockerImageTag)).ToList())
            .Map(dest => dest.RuntimeStack, src => src.RuntimeStack.Value.ToString())
            .Map(dest => dest.AppServicePlanId, src => src.AppServicePlanId.Value)
            .Map(dest => dest.DeploymentMode, src => src.DeploymentMode.Value.ToString())
            .Map(dest => dest.ContainerRegistryId, src => src.ContainerRegistryId != null ? src.ContainerRegistryId.Value : (Guid?)null);

        config.NewConfig<WebAppEnvironmentConfigData, WebAppEnvironmentConfigResponse>()
            .MapWith(src => new WebAppEnvironmentConfigResponse(
                src.EnvironmentName, src.AlwaysOn, src.HttpsOnly, src.RuntimeStack, src.RuntimeVersion, src.DockerImageTag));
    }
}
