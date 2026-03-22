using Shared.Domain.Domain.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ProjectEnvironmentDefinition"/>.</summary>
public sealed class ProjectEnvironmentDefinitionId(Guid value) : Id<ProjectEnvironmentDefinitionId>(value);
