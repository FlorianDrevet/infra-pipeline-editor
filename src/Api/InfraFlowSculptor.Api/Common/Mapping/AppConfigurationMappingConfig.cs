using InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;
using InfraFlowSculptor.Application.AppConfigurations.Commands.UpdateAppConfiguration;
using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Contracts.AppConfigurations.Requests;
using InfraFlowSculptor.Domain.AppConfigurationAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the App Configuration aggregate.</summary>
public class AppConfigurationMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateAppConfigurationRequest, CreateAppConfigurationCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new AppConfigurationEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.SoftDeleteRetentionInDays,
                        ec.PurgeProtectionEnabled,
                        ec.DisableLocalAuth,
                        ec.PublicNetworkAccess)).ToList());

        config.NewConfig<(Guid Id, UpdateAppConfigurationRequest Request), UpdateAppConfigurationCommand>()
            .MapWith(src => new UpdateAppConfigurationCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new AppConfigurationEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.SoftDeleteRetentionInDays,
                        ec.PurgeProtectionEnabled,
                        ec.DisableLocalAuth,
                        ec.PublicNetworkAccess)).ToList()));

        config.NewConfig<AppConfiguration, AppConfigurationResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new AppConfigurationEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku,
                    es.SoftDeleteRetentionInDays,
                    es.PurgeProtectionEnabled,
                    es.DisableLocalAuth,
                    es.PublicNetworkAccess)).ToList());

        config.NewConfig<AppConfigurationEnvironmentConfigData, AppConfigurationEnvironmentConfigResponse>()
            .MapWith(src => new AppConfigurationEnvironmentConfigResponse(
                src.EnvironmentName,
                src.Sku,
                src.SoftDeleteRetentionInDays,
                src.PurgeProtectionEnabled,
                src.DisableLocalAuth,
                src.PublicNetworkAccess));
    }
}
