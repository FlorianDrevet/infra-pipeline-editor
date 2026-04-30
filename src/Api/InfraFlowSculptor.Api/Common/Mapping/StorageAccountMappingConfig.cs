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

public sealed class StorageAccountMappingConfig : IRegister
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Tracked under test-debt #22: refactoring deferred until dedicated unit-test coverage protects against behavioural regressions. The method orchestrates a single coherent business operation and would lose readability without proper test guards.")]
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateStorageAccountRequest, CreateStorageAccountCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new StorageAccountEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku)).ToList())
            .Map(dest => dest.CorsRules,
                src => src.CorsRules == null
                    ? null
                    : src.CorsRules.Select(rule => new CorsRuleResult(
                        rule.AllowedOrigins,
                        rule.AllowedMethods,
                        rule.AllowedHeaders,
                        rule.ExposedHeaders,
                        rule.MaxAgeInSeconds)).ToList())
            .Map(dest => dest.TableCorsRules,
                src => src.TableCorsRules == null
                    ? null
                    : src.TableCorsRules.Select(rule => new CorsRuleResult(
                        rule.AllowedOrigins,
                        rule.AllowedMethods,
                        rule.AllowedHeaders,
                        rule.ExposedHeaders,
                        rule.MaxAgeInSeconds)).ToList())
            .Map(dest => dest.LifecycleRules,
                src => src.LifecycleRules == null
                    ? null
                    : src.LifecycleRules.Select(rule => new BlobLifecycleRuleResult(
                        rule.RuleName,
                        rule.ContainerNames,
                        rule.TimeToLiveInDays)).ToList());

        config.NewConfig<(Guid Id, UpdateStorageAccountRequest Request), UpdateStorageAccountCommand>()
            .MapWith(src => new UpdateStorageAccountCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.Kind,
                src.Request.AccessTier,
                src.Request.AllowBlobPublicAccess,
                src.Request.EnableHttpsTrafficOnly,
                src.Request.MinimumTlsVersion,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new StorageAccountEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku)).ToList(),
                src.Request.CorsRules == null
                    ? null
                    : src.Request.CorsRules.Select(rule => new CorsRuleResult(
                        rule.AllowedOrigins,
                        rule.AllowedMethods,
                        rule.AllowedHeaders,
                        rule.ExposedHeaders,
                        rule.MaxAgeInSeconds)).ToList(),
                src.Request.TableCorsRules == null
                    ? null
                    : src.Request.TableCorsRules.Select(rule => new CorsRuleResult(
                        rule.AllowedOrigins,
                        rule.AllowedMethods,
                        rule.AllowedHeaders,
                        rule.ExposedHeaders,
                        rule.MaxAgeInSeconds)).ToList(),
                src.Request.LifecycleRules == null
                    ? null
                    : src.Request.LifecycleRules.Select(rule => new BlobLifecycleRuleResult(
                        rule.RuleName,
                        rule.ContainerNames,
                        rule.TimeToLiveInDays)).ToList()));

        config.NewConfig<StorageAccount, StorageAccountResult>()
            .Map(dest => dest.Kind, src => src.Kind.Value.ToString())
            .Map(dest => dest.AccessTier, src => src.AccessTier.Value.ToString())
            .Map(dest => dest.MinimumTlsVersion, src => src.MinimumTlsVersion.Value.ToString())
            .Map(dest => dest.CorsRules,
                src => src.CorsRules.Select(rule => new CorsRuleResult(
                    rule.AllowedOrigins,
                    rule.AllowedMethods,
                    rule.AllowedHeaders,
                    rule.ExposedHeaders,
                    rule.MaxAgeInSeconds)).ToList())
            .Map(dest => dest.TableCorsRules,
                src => src.TableCorsRules.Select(rule => new CorsRuleResult(
                    rule.AllowedOrigins,
                    rule.AllowedMethods,
                    rule.AllowedHeaders,
                    rule.ExposedHeaders,
                    rule.MaxAgeInSeconds)).ToList())
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new StorageAccountEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku != null ? es.Sku.Value.ToString() : null)).ToList())
            .Map(dest => dest.LifecycleRules,
                src => src.LifecycleRules.Select(rule => new BlobLifecycleRuleResult(
                    rule.RuleName,
                    rule.ContainerNames,
                    rule.TimeToLiveInDays)).ToList());

        config.NewConfig<StorageAccountEnvironmentConfigData, StorageAccountEnvironmentConfigResponse>()
            .MapWith(src => new StorageAccountEnvironmentConfigResponse(
                src.EnvironmentName, src.Sku));

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
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.PublicAccess, src => src.PublicAccess.Value.ToString());

        config.NewConfig<CorsRuleResult, CorsRuleResponse>();

        config.NewConfig<StorageQueueResult, StorageQueueResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString());

        config.NewConfig<StorageTableResult, StorageTableResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString());

        config.NewConfig<BlobLifecycleRuleResult, BlobLifecycleRuleResponse>();
    }
}
