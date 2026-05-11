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

/// <summary>Mapster mapping configuration for the Storage Account feature.</summary>
public sealed class StorageAccountMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        RegisterCommandMappings(config);
        RegisterStorageAccountMappings(config);
        RegisterValueObjectMappings(config);
        RegisterResponseMappings(config);
    }

    private static void RegisterCommandMappings(TypeAdapterConfig config)
    {
        config.NewConfig<CreateStorageAccountRequest, CreateStorageAccountCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => MapOptionalList(src.EnvironmentSettings, environmentSetting => new StorageAccountEnvironmentConfigData(
                    environmentSetting.EnvironmentName,
                    environmentSetting.Sku)))
            .Map(dest => dest.CorsRules,
                src => MapOptionalList(src.CorsRules, rule => new CorsRuleResult(
                    rule.AllowedOrigins,
                    rule.AllowedMethods,
                    rule.AllowedHeaders,
                    rule.ExposedHeaders,
                    rule.MaxAgeInSeconds)))
            .Map(dest => dest.TableCorsRules,
                src => MapOptionalList(src.TableCorsRules, rule => new CorsRuleResult(
                    rule.AllowedOrigins,
                    rule.AllowedMethods,
                    rule.AllowedHeaders,
                    rule.ExposedHeaders,
                    rule.MaxAgeInSeconds)))
            .Map(dest => dest.LifecycleRules,
                src => MapOptionalList(src.LifecycleRules, rule => new BlobLifecycleRuleResult(
                    rule.RuleName,
                    rule.ContainerNames,
                    rule.TimeToLiveInDays)));

        config.NewConfig<(Guid Id, UpdateStorageAccountRequest Request), UpdateStorageAccountCommand>()
            .MapWith(source => MapUpdateStorageAccountCommand(source));
    }

    private static void RegisterStorageAccountMappings(TypeAdapterConfig config)
    {
        config.NewConfig<StorageAccount, StorageAccountResult>()
            .Map(dest => dest.Kind, src => src.Kind.Value.ToString())
            .Map(dest => dest.AccessTier, src => src.AccessTier.Value.ToString())
            .Map(dest => dest.MinimumTlsVersion, src => src.MinimumTlsVersion.Value.ToString())
            .Map(dest => dest.CorsRules,
                src => MapList(src.GetBlobCorsRules(), rule => new CorsRuleResult(
                    rule.AllowedOrigins,
                    rule.AllowedMethods,
                    rule.AllowedHeaders,
                    rule.ExposedHeaders,
                    rule.MaxAgeInSeconds)))
            .Map(dest => dest.TableCorsRules,
                src => MapList(src.GetTableCorsRules(), rule => new CorsRuleResult(
                    rule.AllowedOrigins,
                    rule.AllowedMethods,
                    rule.AllowedHeaders,
                    rule.ExposedHeaders,
                    rule.MaxAgeInSeconds)))
            .Map(dest => dest.EnvironmentSettings,
                src => MapList(src.EnvironmentSettings, environmentSetting => new StorageAccountEnvironmentConfigData(
                    environmentSetting.EnvironmentName,
                    environmentSetting.Sku != null ? environmentSetting.Sku.Value.ToString() : null)))
            .Map(dest => dest.LifecycleRules,
                src => MapList(src.LifecycleRules, rule => new BlobLifecycleRuleResult(
                    rule.RuleName,
                    rule.ContainerNames,
                    rule.TimeToLiveInDays)));

        config.NewConfig<StorageAccountEnvironmentConfigData, StorageAccountEnvironmentConfigResponse>()
            .MapWith(src => new StorageAccountEnvironmentConfigResponse(
                src.EnvironmentName,
                src.Sku));
    }

    private static void RegisterValueObjectMappings(TypeAdapterConfig config)
    {
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
    }

    private static void RegisterResponseMappings(TypeAdapterConfig config)
    {
        config.NewConfig<BlobContainerResult, BlobContainerResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.PublicAccess, src => src.PublicAccess.Value.ToString());

        config.NewConfig<CorsRuleResult, CorsRuleResponse>();

        config.NewConfig<StorageQueueResult, StorageQueueResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString());

        config.NewConfig<StorageTableResult, StorageTableResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString());

        config.NewConfig<BlobLifecycleRuleResult, BlobLifecycleRuleResponse>();
    }

    private static UpdateStorageAccountCommand MapUpdateStorageAccountCommand((Guid Id, UpdateStorageAccountRequest Request) source)
    {
        return new UpdateStorageAccountCommand(
            source.Id.Adapt<AzureResourceId>(),
            source.Request.Name.Adapt<Name>(),
            source.Request.Location.Adapt<Location>(),
            source.Request.Kind,
            source.Request.AccessTier,
            source.Request.AllowBlobPublicAccess,
            source.Request.EnableHttpsTrafficOnly,
            source.Request.MinimumTlsVersion,
            MapOptionalList(source.Request.EnvironmentSettings, environmentSetting => new StorageAccountEnvironmentConfigData(
                environmentSetting.EnvironmentName,
                environmentSetting.Sku)),
            MapOptionalList(source.Request.CorsRules, rule => new CorsRuleResult(
                rule.AllowedOrigins,
                rule.AllowedMethods,
                rule.AllowedHeaders,
                rule.ExposedHeaders,
                rule.MaxAgeInSeconds)),
            MapOptionalList(source.Request.TableCorsRules, rule => new CorsRuleResult(
                rule.AllowedOrigins,
                rule.AllowedMethods,
                rule.AllowedHeaders,
                rule.ExposedHeaders,
                rule.MaxAgeInSeconds)),
            MapOptionalList(source.Request.LifecycleRules, rule => new BlobLifecycleRuleResult(
                rule.RuleName,
                rule.ContainerNames,
                rule.TimeToLiveInDays)));
    }

    private static List<TResult>? MapOptionalList<TSource, TResult>(
        IEnumerable<TSource>? source,
        Func<TSource, TResult> map)
    {
        return source == null
            ? null
            : source.Select(map).ToList();
    }

    private static List<TResult> MapList<TSource, TResult>(
        IEnumerable<TSource> source,
        Func<TSource, TResult> map)
    {
        return source.Select(map).ToList();
    }
}
