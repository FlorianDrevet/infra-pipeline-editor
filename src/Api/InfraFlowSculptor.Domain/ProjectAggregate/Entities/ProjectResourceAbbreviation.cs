using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Represents a project-level override for a resource type abbreviation
/// used in the <c>{resourceAbbr}</c> naming-template placeholder.
/// </summary>
public sealed class ProjectResourceAbbreviation : Entity<ProjectResourceAbbreviationId>
{
    /// <summary>Gets the identifier of the parent project.</summary>
    public ProjectId ProjectId { get; private set; } = null!;

    /// <summary>Gets the parent project navigation property.</summary>
    public Project Project { get; private set; } = null!;

    /// <summary>Gets the Azure resource type this abbreviation applies to.</summary>
    public string ResourceType { get; private set; } = null!;

    /// <summary>Gets the custom abbreviation value.</summary>
    public string Abbreviation { get; private set; } = null!;

    /// <summary>EF Core constructor.</summary>
    private ProjectResourceAbbreviation() { }

    /// <summary>
    /// Creates a new <see cref="ProjectResourceAbbreviation"/> for the specified resource type.
    /// </summary>
    internal ProjectResourceAbbreviation(ProjectId projectId, string resourceType, string abbreviation)
        : base(ProjectResourceAbbreviationId.CreateUnique())
    {
        ProjectId = projectId;
        ResourceType = resourceType;
        Abbreviation = abbreviation;
    }

    /// <summary>Updates the abbreviation value.</summary>
    internal void Update(string abbreviation) => Abbreviation = abbreviation;
}
