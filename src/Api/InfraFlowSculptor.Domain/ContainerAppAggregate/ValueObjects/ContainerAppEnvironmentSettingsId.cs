using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ContainerAppAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ContainerAppEnvironmentSettings"/>.</summary>
public sealed class ContainerAppEnvironmentSettingsId(Guid value) : Id<ContainerAppEnvironmentSettingsId>(value);
