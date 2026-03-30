using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.AppConfigurationKeyEnvironmentValue"/>.</summary>
public sealed class AppConfigurationKeyEnvironmentValueId(Guid value) : Id<AppConfigurationKeyEnvironmentValueId>(value);
