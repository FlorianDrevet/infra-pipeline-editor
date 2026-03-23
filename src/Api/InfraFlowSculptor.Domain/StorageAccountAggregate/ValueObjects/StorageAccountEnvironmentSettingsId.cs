using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.StorageAccountEnvironmentSettings"/>.</summary>
public sealed class StorageAccountEnvironmentSettingsId(Guid value) : Id<StorageAccountEnvironmentSettingsId>(value);
