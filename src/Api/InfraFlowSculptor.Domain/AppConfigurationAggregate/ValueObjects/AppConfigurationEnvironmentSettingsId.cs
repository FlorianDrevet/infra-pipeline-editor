using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.AppConfigurationEnvironmentSettings"/>.</summary>
public sealed class AppConfigurationEnvironmentSettingsId(Guid value) : Id<AppConfigurationEnvironmentSettingsId>(value);
