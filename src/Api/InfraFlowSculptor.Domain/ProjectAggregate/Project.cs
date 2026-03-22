using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Shared.Domain.Domain.Models;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.ProjectAggregate;

/// <summary>
/// Represents a project that groups multiple infrastructure configurations
/// and centralizes member access control.
/// </summary>
public sealed class Project : AggregateRoot<ProjectId>
{
    /// <summary>Gets the name of the project.</summary>
    public Name Name { get; private set; } = null!;

    /// <summary>Gets the optional description of the project.</summary>
    public string? Description { get; private set; }

    private readonly List<ProjectMember> _members = new();

    /// <summary>Gets the members of this project with their roles.</summary>
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

    private Project(ProjectId id, Name name, string? description, UserId ownerId) : base(id)
    {
        Name = name;
        Description = description;
        _members.Add(ProjectMember.CreateOwner(id, ownerId));
    }

    /// <summary>
    /// Creates a new <see cref="Project"/> with a generated identifier.
    /// The caller is automatically added as Owner.
    /// </summary>
    public static Project Create(Name name, string? description, UserId ownerId)
        => new(ProjectId.CreateUnique(), name, description, ownerId);

    /// <summary>EF Core constructor.</summary>
    public Project() { }

    /// <summary>Adds a member with the specified role to this project.</summary>
    public void AddMember(UserId userId, Role role)
    {
        _members.Add(new ProjectMember(Id, userId, role));
    }

    /// <summary>Changes the role of an existing member.</summary>
    public void ChangeRole(UserId userId, Role newRole)
    {
        var member = GetMember(userId);
        member?.ChangeRole(newRole);
    }

    /// <summary>Removes a member from this project.</summary>
    public void RemoveMember(UserId userId)
    {
        var member = GetMember(userId);
        if (member is not null)
            _members.Remove(member);
    }

    private ProjectMember? GetMember(UserId userId)
        => _members.FirstOrDefault(m => m.UserId == userId);
}
