using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.AppConfigurationKey"/>.</summary>
public sealed class AppConfigurationKeyId(Guid value) : Id<AppConfigurationKeyId>(value);
