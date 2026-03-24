using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ProjectAggregate.Entities;

/// <summary>
/// Represents a user's membership in a <see cref="Project"/> with a specific <see cref="Role"/>.
/// </summary>
public sealed class ProjectMember : Entity<ProjectMemberId>
{
    /// <summary>Gets the user identifier.</summary>
    public UserId UserId { get; private set; }

    /// <summary>Gets the role assigned to this member.</summary>
    public Role Role { get; private set; }

    /// <summary>Gets the parent project identifier.</summary>
    public ProjectId ProjectId { get; set; } = null!;

    /// <summary>Navigation property to the parent project.</summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Navigation property to the associated <see cref="UserAggregate.User"/>.
    /// Populated by EF Core when the query includes the User relationship.
    /// </summary>
    public User? User { get; private set; }

    private ProjectMember() { }

    internal ProjectMember(ProjectId projectId, UserId userId, Role role)
        : base(ProjectMemberId.CreateUnique())
    {
        UserId = userId;
        Role = role;
        ProjectId = projectId;
    }

    /// <summary>Creates a new owner member for the given project.</summary>
    internal static ProjectMember CreateOwner(ProjectId projectId, UserId userId)
        => new(projectId, userId, new Role(Role.RoleEnum.Owner));

    /// <summary>Changes the role of this member.</summary>
    internal void ChangeRole(Role role)
    {
        Role = role;
    }
}
