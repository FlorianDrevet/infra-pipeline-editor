using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ParameterDefinition;

/// <summary>Strongly-typed identifier for a <see cref="Entities.ParameterDefinition"/>.</summary>
public sealed class ParameterDefinitionId(Guid value) : Id<ParameterDefinitionId>(value);