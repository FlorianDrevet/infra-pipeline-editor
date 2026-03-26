using ErrorOr;
using InfraFlowSculptor.Application.StorageAccounts.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;

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
    IReadOnlyList<StorageAccountEnvironmentConfigData>? EnvironmentSettings = null,
    IReadOnlyList<CorsRuleResult>? CorsRules = null,
    IReadOnlyList<CorsRuleResult>? TableCorsRules = null
) : IRequest<ErrorOr<StorageAccountResult>>;
