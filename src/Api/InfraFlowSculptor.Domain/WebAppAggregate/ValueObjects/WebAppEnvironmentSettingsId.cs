using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.WebAppAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.WebAppEnvironmentSettings"/>.</summary>
public sealed class WebAppEnvironmentSettingsId(Guid value) : Id<WebAppEnvironmentSettingsId>(value);
