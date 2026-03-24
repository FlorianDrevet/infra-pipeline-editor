using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ContainerAppEnvironmentEnvironmentSettings"/>.</summary>
public sealed class ContainerAppEnvironmentEnvironmentSettingsId(Guid value) : Id<ContainerAppEnvironmentEnvironmentSettingsId>(value);
