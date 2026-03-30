using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.ProjectAggregate;

/// <summary>
/// Groups all fields needed to create or update a <see cref="Entities.ProjectEnvironmentDefinition"/>,
/// reducing method parameter count complexity.
/// </summary>
public record EnvironmentDefinitionData(
    Name Name,
    ShortName ShortName,
    Prefix Prefix,
    Suffix Suffix,
    Location Location,
    SubscriptionId SubscriptionId,
    Order Order,
    RequiresApproval RequiresApproval,
    string? AzureResourceManagerConnection,
    IEnumerable<Tag> Tags);
