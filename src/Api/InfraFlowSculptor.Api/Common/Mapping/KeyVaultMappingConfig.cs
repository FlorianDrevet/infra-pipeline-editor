using InfraFlowSculptor.Application.Common;
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

public class KeyVaultMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateKeyVaultRequest, CreateKeyVaultCommand>();

        config.NewConfig<(Guid Id, UpdateKeyVaultRequest Request), UpdateKeyVaultCommand>()
            .MapWith(src => new UpdateKeyVaultCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.Sku.Adapt<Sku>(),
                src.Request.EnvironmentConfigs == null
                    ? null
                    : src.Request.EnvironmentConfigs.Select(ec => new EnvironmentConfigData(
                        ec.EnvironmentName, ec.Properties)).ToList()));

        config.NewConfig<KeyVault, KeyVaultResult>()
            .Map(dest => dest.EnvironmentConfigs,
                src => src.EnvironmentConfigs.Select(ec => new EnvironmentConfigData(
                    ec.EnvironmentName, ec.Properties)).ToList());

        config.NewConfig<Sku, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, Sku>()
            .MapWith(src => new Sku(Enum.Parse<Sku.SkuEnum>(src)));
    }
}
