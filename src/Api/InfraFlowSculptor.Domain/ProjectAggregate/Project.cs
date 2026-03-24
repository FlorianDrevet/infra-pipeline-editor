using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Domain.ProjectAggregate;

/// <summary>
/// Represents a project that groups multiple infrastructure configurations
/// and centralizes member access control, default environments, and naming conventions.
/// </summary>
public sealed class Project : AggregateRoot<ProjectId>
{
    /// <summary>Gets the name of the project.</summary>
    public Name Name { get; private set; } = null!;

    /// <summary>Gets the optional description of the project.</summary>
    public string? Description { get; private set; }

    // ─── Members ────────────────────────────────────────────────────────────

    private readonly List<ProjectMember> _members = new();

    /// <summary>Gets the members of this project with their roles.</summary>
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

    // ─── Environment Definitions ────────────────────────────────────────────

    private readonly List<ProjectEnvironmentDefinition> _environmentDefinitions = new();

    /// <summary>Gets the project-level environment definitions.</summary>
    public IReadOnlyCollection<ProjectEnvironmentDefinition> EnvironmentDefinitions
        => _environmentDefinitions.AsReadOnly();

    // ─── Naming Conventions ─────────────────────────────────────────────────

    /// <summary>
    /// Default naming template applied to all resource types unless overridden.
    /// Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {resourceAbbr}, {location}.
    /// When null, the resource Name is used as-is.
    /// </summary>
    public NamingTemplate? DefaultNamingTemplate { get; private set; }

    private readonly List<ProjectResourceNamingTemplate> _resourceNamingTemplates = new();

    /// <summary>Gets the project-level per-resource-type naming template overrides.</summary>
    public IReadOnlyCollection<ProjectResourceNamingTemplate> ResourceNamingTemplates
        => _resourceNamingTemplates.AsReadOnly();

    // ─── Git Repository Configuration ───────────────────────────────────────

    /// <summary>Gets the optional Git repository configuration for pushing generated Bicep files.</summary>
    public GitRepositoryConfiguration? GitRepositoryConfiguration { get; private set; }

    // ─── Constructor ────────────────────────────────────────────────────────

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

    // ─── Member Management ──────────────────────────────────────────────────

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

    // ─── Environment Definitions Management ─────────────────────────────────

    /// <summary>Adds a new environment definition at the specified order.</summary>
    public ProjectEnvironmentDefinition AddEnvironment(EnvironmentDefinitionData data)
    {
        ShiftOrdersUp(data.Order.Value);
        var env = new ProjectEnvironmentDefinition(Id, data);
        _environmentDefinitions.Add(env);
        return env;
    }

    /// <summary>Updates an existing environment definition.</summary>
    public ProjectEnvironmentDefinition? UpdateEnvironment(
        ProjectEnvironmentDefinitionId envId,
        EnvironmentDefinitionData data)
    {
        var env = _environmentDefinitions.FirstOrDefault(e => e.Id == envId);
        if (env is null)
            return null;

        var oldOrder = env.Order.Value;
        var newOrder = data.Order.Value;

        if (oldOrder != newOrder)
            ReorderEnvironments(envId, oldOrder, newOrder);

        env.Name = data.Name;
        env.ShortName = data.ShortName;
        env.Prefix = data.Prefix;
        env.Suffix = data.Suffix;
        env.Location = data.Location;
        env.TenantId = data.TenantId;
        env.SubscriptionId = data.SubscriptionId;
        env.Order = data.Order;
        env.RequiresApproval = data.RequiresApproval;
        env.SetTags(data.Tags);
        return env;
    }

    /// <summary>Removes an environment definition by its identifier.</summary>
    public bool RemoveEnvironment(ProjectEnvironmentDefinitionId envId)
    {
        var env = _environmentDefinitions.FirstOrDefault(e => e.Id == envId);
        if (env is null)
            return false;

        var removedOrder = env.Order.Value;
        _environmentDefinitions.Remove(env);
        ShiftOrdersDown(removedOrder);
        return true;
    }

    private void ShiftOrdersUp(int fromOrder)
    {
        foreach (var env in _environmentDefinitions.Where(e => e.Order.Value >= fromOrder))
            env.Order = new Order(env.Order.Value + 1);
    }

    private void ShiftOrdersDown(int removedOrder)
    {
        foreach (var env in _environmentDefinitions.Where(e => e.Order.Value > removedOrder))
            env.Order = new Order(env.Order.Value - 1);
    }

    private void ReorderEnvironments(ProjectEnvironmentDefinitionId movingId, int oldOrder, int newOrder)
    {
        if (newOrder < oldOrder)
        {
            foreach (var e in _environmentDefinitions
                         .Where(e => e.Id != movingId && e.Order.Value >= newOrder && e.Order.Value < oldOrder))
                e.Order = new Order(e.Order.Value + 1);
        }
        else
        {
            foreach (var e in _environmentDefinitions
                         .Where(e => e.Id != movingId && e.Order.Value > oldOrder && e.Order.Value <= newOrder))
                e.Order = new Order(e.Order.Value - 1);
        }
    }

    // ─── Naming Convention Management ───────────────────────────────────────

    /// <summary>Sets or clears the project-level default naming template.</summary>
    public void SetDefaultNamingTemplate(NamingTemplate? template)
    {
        DefaultNamingTemplate = template;
    }

    /// <summary>Sets or updates a per-resource-type naming template.</summary>
    public ProjectResourceNamingTemplate SetResourceNamingTemplate(string resourceType, NamingTemplate template)
    {
        var existing = _resourceNamingTemplates.FirstOrDefault(t => t.ResourceType == resourceType);
        if (existing is not null)
        {
            existing.Update(template);
            return existing;
        }

        var entry = new ProjectResourceNamingTemplate(Id, resourceType, template);
        _resourceNamingTemplates.Add(entry);
        return entry;
    }

    /// <summary>Removes a per-resource-type naming template.</summary>
    public bool RemoveResourceNamingTemplate(string resourceType)
    {
        var existing = _resourceNamingTemplates.FirstOrDefault(t => t.ResourceType == resourceType);
        if (existing is null)
            return false;
        _resourceNamingTemplates.Remove(existing);
        return true;
    }

    // ─── Git Repository Configuration Management ────────────────────────────

    /// <summary>Sets or updates the Git repository configuration for this project.</summary>
    public GitRepositoryConfiguration SetGitRepositoryConfiguration(
        GitProviderType providerType,
        string repositoryUrl,
        string defaultBranch,
        string? basePath)
    {
        if (GitRepositoryConfiguration is not null)
        {
            GitRepositoryConfiguration.Update(providerType, repositoryUrl, defaultBranch, basePath);
            return GitRepositoryConfiguration;
        }

        GitRepositoryConfiguration = Entities.GitRepositoryConfiguration.Create(
            providerType, repositoryUrl, defaultBranch, basePath, Id);
        return GitRepositoryConfiguration;
    }

    /// <summary>Removes the Git repository configuration from this project.</summary>
    public ErrorOr<Deleted> RemoveGitRepositoryConfiguration()
    {
        if (GitRepositoryConfiguration is null)
            return Domain.Common.Errors.Errors.GitRepository.NotConfigured();

        GitRepositoryConfiguration = null;
        return Result.Deleted;
    }
}
