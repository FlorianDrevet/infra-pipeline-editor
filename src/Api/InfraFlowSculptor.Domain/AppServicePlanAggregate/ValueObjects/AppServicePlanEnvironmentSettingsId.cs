using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.AppServicePlanEnvironmentSettings"/>.</summary>
public sealed class AppServicePlanEnvironmentSettingsId(Guid value) : Id<AppServicePlanEnvironmentSettingsId>(value);
