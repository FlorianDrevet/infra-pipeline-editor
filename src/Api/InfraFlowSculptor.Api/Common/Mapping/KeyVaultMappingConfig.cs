using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Common;
using InfraFlowSculptor.Contracts.KeyVaults.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public sealed class KeyVaultMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateKeyVaultRequest, CreateKeyVaultCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new KeyVaultEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku)).ToList());

        config.NewConfig<(Guid Id, UpdateKeyVaultRequest Request), UpdateKeyVaultCommand>()
            .MapWith(src => new UpdateKeyVaultCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.EnableRbacAuthorization,
                src.Request.EnabledForDeployment,
                src.Request.EnabledForDiskEncryption,
                src.Request.EnabledForTemplateDeployment,
                src.Request.EnablePurgeProtection,
                src.Request.EnableSoftDelete,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new KeyVaultEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku)).ToList()));

        config.NewConfig<KeyVault, KeyVaultResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new KeyVaultEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku != null ? es.Sku.Value.ToString() : null)).ToList());

        config.NewConfig<KeyVaultEnvironmentConfigData, KeyVaultEnvironmentConfigResponse>()
            .MapWith(src => new KeyVaultEnvironmentConfigResponse(
                src.EnvironmentName, src.Sku));

        config.NewConfig<Sku, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, Sku>()
            .MapWith(src => new Sku(Enum.Parse<Sku.SkuEnum>(src)));
    }
}
