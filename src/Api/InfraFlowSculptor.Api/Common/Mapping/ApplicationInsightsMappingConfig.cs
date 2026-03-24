using InfraFlowSculptor.Application.ApplicationInsights.Commands.CreateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Commands.UpdateApplicationInsights;
using InfraFlowSculptor.Application.ApplicationInsights.Common;
using InfraFlowSculptor.Contracts.ApplicationInsights.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Application Insights feature.</summary>
public sealed class ApplicationInsightsMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateApplicationInsightsRequest, CreateApplicationInsightsCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new ApplicationInsightsEnvironmentConfigData(
                        ec.EnvironmentName, ec.SamplingPercentage, ec.RetentionInDays, ec.DisableIpMasking, ec.DisableLocalAuth, ec.IngestionMode)).ToList());

        config.NewConfig<(Guid Id, UpdateApplicationInsightsRequest Request), UpdateApplicationInsightsCommand>()
            .MapWith(src => new UpdateApplicationInsightsCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.LogAnalyticsWorkspaceId,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new ApplicationInsightsEnvironmentConfigData(
                        ec.EnvironmentName, ec.SamplingPercentage, ec.RetentionInDays, ec.DisableIpMasking, ec.DisableLocalAuth, ec.IngestionMode)).ToList()));

        config.NewConfig<Domain.ApplicationInsightsAggregate.ApplicationInsights, ApplicationInsightsResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new ApplicationInsightsEnvironmentConfigData(
                    es.EnvironmentName,
                    es.SamplingPercentage,
                    es.RetentionInDays,
                    es.DisableIpMasking,
                    es.DisableLocalAuth,
                    es.IngestionMode)).ToList())
            .Map(dest => dest.LogAnalyticsWorkspaceId, src => src.LogAnalyticsWorkspaceId.Value);

        config.NewConfig<ApplicationInsightsEnvironmentConfigData, ApplicationInsightsEnvironmentConfigResponse>()
            .MapWith(src => new ApplicationInsightsEnvironmentConfigResponse(
                src.EnvironmentName, src.SamplingPercentage, src.RetentionInDays, src.DisableIpMasking, src.DisableLocalAuth, src.IngestionMode));
    }
}
