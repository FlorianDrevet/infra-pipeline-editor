using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.CosmosDbAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.CosmosDbEnvironmentSettings"/>.</summary>
public sealed class CosmosDbEnvironmentSettingsId(Guid value) : Id<CosmosDbEnvironmentSettingsId>(value);
