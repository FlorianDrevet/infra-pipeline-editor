using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.CreateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Commands.UpdateLogAnalyticsWorkspace;
using InfraFlowSculptor.Application.LogAnalyticsWorkspaces.Common;
using InfraFlowSculptor.Contracts.LogAnalyticsWorkspaces.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.LogAnalyticsWorkspaceAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Log Analytics Workspace aggregate.</summary>
public sealed class LogAnalyticsWorkspaceMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateLogAnalyticsWorkspaceRequest, CreateLogAnalyticsWorkspaceCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new LogAnalyticsWorkspaceEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.RetentionInDays,
                        ec.DailyQuotaGb)).ToList());

        config.NewConfig<(Guid Id, UpdateLogAnalyticsWorkspaceRequest Request), UpdateLogAnalyticsWorkspaceCommand>()
            .MapWith(src => new UpdateLogAnalyticsWorkspaceCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new LogAnalyticsWorkspaceEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.RetentionInDays,
                        ec.DailyQuotaGb)).ToList()));

        config.NewConfig<LogAnalyticsWorkspace, LogAnalyticsWorkspaceResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new LogAnalyticsWorkspaceEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku,
                    es.RetentionInDays,
                    es.DailyQuotaGb)).ToList());

        config.NewConfig<LogAnalyticsWorkspaceEnvironmentConfigData, LogAnalyticsWorkspaceEnvironmentConfigResponse>()
            .MapWith(src => new LogAnalyticsWorkspaceEnvironmentConfigResponse(
                src.EnvironmentName,
                src.Sku,
                src.RetentionInDays,
                src.DailyQuotaGb));
    }
}
