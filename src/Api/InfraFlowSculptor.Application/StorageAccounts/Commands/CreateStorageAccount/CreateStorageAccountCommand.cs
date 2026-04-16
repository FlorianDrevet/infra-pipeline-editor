using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.StorageAccounts.Commands.CreateStorageAccount;

public record CreateStorageAccountCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    string Kind,
    string AccessTier,
    bool AllowBlobPublicAccess,
    bool EnableHttpsTrafficOnly,
    string MinimumTlsVersion,
    IReadOnlyCollection<StorageAccountEnvironmentConfigData>? EnvironmentSettings = null,
    IReadOnlyCollection<CorsRuleResult>? CorsRules = null,
    IReadOnlyCollection<CorsRuleResult>? TableCorsRules = null,
    IReadOnlyCollection<BlobLifecycleRuleResult>? LifecycleRules = null
) : ICommand<StorageAccountResult>;
