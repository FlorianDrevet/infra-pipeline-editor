using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.FrontDoorEnvironmentSettings"/>.</summary>
public sealed class FrontDoorEnvironmentSettingsId(Guid value) : Id<FrontDoorEnvironmentSettingsId>(value);
