using InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Commands.UpdateStorageAccount;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Contracts.StorageAccounts.Requests;
using InfraFlowSculptor.Contracts.StorageAccounts.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.StorageAccountAggregate;
using InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

public class StorageAccountMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateStorageAccountRequest, CreateStorageAccountCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new StorageAccountEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku, ec.Kind, ec.AccessTier,
                        ec.AllowBlobPublicAccess, ec.EnableHttpsTrafficOnly, ec.MinimumTlsVersion)).ToList());

        config.NewConfig<(Guid Id, UpdateStorageAccountRequest Request), UpdateStorageAccountCommand>()
            .MapWith(src => new UpdateStorageAccountCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.Sku.Adapt<StorageAccountSku>(),
                src.Request.Kind.Adapt<StorageAccountKind>(),
                src.Request.AccessTier.Adapt<StorageAccessTier>(),
                src.Request.AllowBlobPublicAccess,
                src.Request.EnableHttpsTrafficOnly,
                src.Request.MinimumTlsVersion.Adapt<StorageAccountTlsVersion>(),
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new StorageAccountEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku, ec.Kind, ec.AccessTier,
                        ec.AllowBlobPublicAccess, ec.EnableHttpsTrafficOnly, ec.MinimumTlsVersion)).ToList()));

        config.NewConfig<StorageAccount, StorageAccountResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new StorageAccountEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku != null ? es.Sku.Value.ToString() : null,
                    es.Kind != null ? es.Kind.Value.ToString() : null,
                    es.AccessTier != null ? es.AccessTier.Value.ToString() : null,
                    es.AllowBlobPublicAccess,
                    es.EnableHttpsTrafficOnly,
                    es.MinimumTlsVersion != null ? es.MinimumTlsVersion.Value.ToString() : null)).ToList());

        config.NewConfig<StorageAccountEnvironmentConfigData, StorageAccountEnvironmentConfigResponse>()
            .MapWith(src => new StorageAccountEnvironmentConfigResponse(
                src.EnvironmentName, src.Sku, src.Kind, src.AccessTier,
                src.AllowBlobPublicAccess, src.EnableHttpsTrafficOnly, src.MinimumTlsVersion));

        config.NewConfig<StorageAccountSku, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, StorageAccountSku>()
            .MapWith(src => new StorageAccountSku(Enum.Parse<StorageAccountSku.Sku>(src)));

        config.NewConfig<StorageAccountKind, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, StorageAccountKind>()
            .MapWith(src => new StorageAccountKind(Enum.Parse<StorageAccountKind.Kind>(src)));

        config.NewConfig<StorageAccessTier, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, StorageAccessTier>()
            .MapWith(src => new StorageAccessTier(Enum.Parse<StorageAccessTier.Tier>(src)));

        config.NewConfig<StorageAccountTlsVersion, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, StorageAccountTlsVersion>()
            .MapWith(src => new StorageAccountTlsVersion(Enum.Parse<StorageAccountTlsVersion.Version>(src)));

        config.NewConfig<BlobContainerPublicAccess, string>()
            .MapWith(src => src.Value.ToString());

        config.NewConfig<string, BlobContainerPublicAccess>()
            .MapWith(src => new BlobContainerPublicAccess(Enum.Parse<BlobContainerPublicAccess.AccessLevel>(src)));

        config.NewConfig<BlobContainerResult, BlobContainerResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PublicAccess, src => src.PublicAccess.Value.ToString());

        config.NewConfig<StorageQueueResult, StorageQueueResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<StorageTableResult, StorageTableResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}
