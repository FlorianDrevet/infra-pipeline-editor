using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ContainerRegistryAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ContainerRegistryEnvironmentSettings"/>.</summary>
public sealed class ContainerRegistryEnvironmentSettingsId(Guid value) : Id<ContainerRegistryEnvironmentSettingsId>(value);
