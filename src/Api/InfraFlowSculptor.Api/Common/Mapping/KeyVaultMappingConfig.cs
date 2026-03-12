using InfraFlowSculptor.Application.KeyVaults.Commands.CreateKeyVault;
using InfraFlowSculptor.Application.KeyVaults.Commands.UpdateKeyVault;
using InfraFlowSculptor.Contracts.KeyVaults.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
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
                src.Request.Sku.Adapt<Sku>()));

        config.NewConfig<Sku, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, Sku>()
            .MapWith(src => new Sku(Enum.Parse<Sku.SkuEnum>(src)));
    }
}
