using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.RedisCacheAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.RedisCacheEnvironmentSettings"/>.</summary>
public sealed class RedisCacheEnvironmentSettingsId(Guid value) : Id<RedisCacheEnvironmentSettingsId>(value);
