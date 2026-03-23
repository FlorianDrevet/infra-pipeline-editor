using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Strongly-typed identifier for <see cref="Entities.ProjectResourceNamingTemplate"/>.</summary>
public sealed class ProjectResourceNamingTemplateId(Guid value) : Id<ProjectResourceNamingTemplateId>(value);
