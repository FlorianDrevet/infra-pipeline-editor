using Shared.Domain.Domain.Models;
using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

/// <summary>Role assigned to a <see cref="Entities.ProjectMember"/> within a <see cref="Project"/>.</summary>
public class ProjectRole(ProjectRole.ProjectRoleEnum value) : EnumValueObject<ProjectRole.ProjectRoleEnum>(value)
{
    /// <summary>Available roles for project membership.</summary>
    public enum ProjectRoleEnum
    {
        /// <summary>Full control over the project and its configurations.</summary>
        Owner,

        /// <summary>Can manage configurations and members but cannot delete the project.</summary>
        Contributor,

        /// <summary>Read-only access to the project and its configurations.</summary>
        Reader,
    }
}
