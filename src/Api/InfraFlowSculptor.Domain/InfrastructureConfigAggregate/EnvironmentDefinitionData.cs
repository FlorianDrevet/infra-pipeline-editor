using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate;

/// <summary>
/// Groups all fields needed to create or update an <see cref="Entities.EnvironmentDefinition"/>,
/// reducing method parameter count complexity.
/// </summary>
public record EnvironmentDefinitionData(
    Name Name,
    Prefix Prefix,
    Suffix Suffix,
    Location Location,
    TenantId TenantId,
    SubscriptionId SubscriptionId,
    Order Order,
    RequiresApproval RequiresApproval,
    IEnumerable<Tag> Tags);
