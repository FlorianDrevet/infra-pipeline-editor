using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.FunctionAppAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.FunctionAppEnvironmentSettings"/>.</summary>
public sealed class FunctionAppEnvironmentSettingsId(Guid value) : Id<FunctionAppEnvironmentSettingsId>(value);
