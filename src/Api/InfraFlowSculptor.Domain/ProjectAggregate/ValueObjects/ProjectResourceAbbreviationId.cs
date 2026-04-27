using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ProjectResourceAbbreviation"/>.</summary>
public sealed class ProjectResourceAbbreviationId(Guid value) : Id<ProjectResourceAbbreviationId>(value);
