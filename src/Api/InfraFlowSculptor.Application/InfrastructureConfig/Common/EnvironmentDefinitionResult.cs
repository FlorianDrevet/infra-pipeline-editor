using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Common;

public record EnvironmentDefinitionResult(
    EnvironmentDefinitionId Id,
    Name Name,
    string Prefix,
    string Suffix,
    string Location,
    Guid TenantId,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval,
    IReadOnlyList<TagResult> Tags);

public record TagResult(string Name, string Value);
