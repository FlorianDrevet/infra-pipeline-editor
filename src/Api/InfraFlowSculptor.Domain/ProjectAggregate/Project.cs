using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using Shared.Domain.Domain.Models;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.ProjectAggregate;

/// <summary>
/// Represents the Project aggregate root.
/// A project groups multiple <see cref="InfrastructureConfig"/> instances
/// and provides project-level access control through <see cref="ProjectMember"/> entities.
/// </summary>
public sealed class Project : AggregateRoot<ProjectId>
{
    /// <summary>Gets the name of the project.</summary>
    public Name Name { get; private set; } = null!;

    /// <summary>Gets the optional description of the project.</summary>
    public Name? Description { get; private set; }

    private readonly List<ProjectMember> _members = new();

    /// <summary>Gets the project members with their assigned roles.</summary>
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

    private readonly List<InfrastructureConfig> _configurations = new();

    /// <summary>Gets the infrastructure configurations associated with this project.</summary>
    public IReadOnlyList<InfrastructureConfig> Configurations => _configurations.AsReadOnly();

    private Project(ProjectId id, Name name, Name? description, UserId ownerId) : base(id)
    {
        Name = name;
        Description = description;
        _members.Add(ProjectMember.CreateOwner(id, ownerId));
    }

    /// <summary>
    /// Creates a new <see cref="Project"/> with the current user as Owner.
    /// </summary>
    public static Project Create(Name name, Name? description, UserId ownerId)
    {
        return new Project(ProjectId.CreateUnique(), name, description, ownerId);
    }

    /// <summary>EF Core parameterless constructor.</summary>
    public Project() { }

    // ─── Name / Description ─────────────────────────────────────────────

    /// <summary>Renames the project.</summary>
    public void Rename(Name name)
    {
        Name = name;
    }

    /// <summary>Updates the project description.</summary>
    public void SetDescription(Name? description)
    {
        Description = description;
    }

    // ─── Member Management ──────────────────────────────────────────────

    /// <summary>Adds a new member with the specified role.</summary>
    public void AddMember(UserId userId, ProjectRole role)
    {
        _members.Add(new ProjectMember(Id, userId, role));
    }

    /// <summary>Changes the role of an existing member.</summary>
    public void ChangeRole(UserId userId, ProjectRole newRole)
    {
        var member = GetMember(userId);
        member?.ChangeRole(newRole);
    }

    /// <summary>Removes a member from the project.</summary>
    public void RemoveMember(UserId userId)
    {
        var member = GetMember(userId);
        if (member is not null)
            _members.Remove(member);
    }

    private ProjectMember? GetMember(UserId userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId);
    }

    // ─── Configuration Association ──────────────────────────────────────

    /// <summary>Associates an infrastructure configuration with this project.</summary>
    public bool AddConfiguration(InfrastructureConfig config)
    {
        if (_configurations.Any(c => c.Id == config.Id))
            return false;

        _configurations.Add(config);
        return true;
    }

    /// <summary>Removes an infrastructure configuration from this project.</summary>
    public bool RemoveConfiguration(InfrastructureConfig config)
    {
        return _configurations.Remove(config);
    }
}
