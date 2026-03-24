using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ApplicationInsightsAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ApplicationInsightsEnvironmentSettings"/>.</summary>
public sealed class ApplicationInsightsEnvironmentSettingsId(Guid value) : Id<ApplicationInsightsEnvironmentSettingsId>(value);
