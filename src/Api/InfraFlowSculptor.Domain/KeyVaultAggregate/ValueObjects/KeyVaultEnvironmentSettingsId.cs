using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.KeyVaultAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.KeyVaultEnvironmentSettings"/>.</summary>
public sealed class KeyVaultEnvironmentSettingsId(Guid value) : Id<KeyVaultEnvironmentSettingsId>(value);
