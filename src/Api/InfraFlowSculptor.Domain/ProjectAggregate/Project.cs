using ErrorOr;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.Entities;
using InfraFlowSculptor.Domain.ProjectAggregate.Events;
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

    // â”€â”€â”€ Members â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private readonly List<ProjectMember> _members = [];

    /// <summary>Gets the members of this project with their roles.</summary>
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

    // â”€â”€â”€ Environment Definitions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private readonly List<ProjectEnvironmentDefinition> _environmentDefinitions = [];

    /// <summary>Gets the project-level environment definitions.</summary>
    public IReadOnlyCollection<ProjectEnvironmentDefinition> EnvironmentDefinitions
        => _environmentDefinitions.AsReadOnly();

    // â”€â”€â”€ Naming Conventions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Default naming template applied to all resource types unless overridden.
    /// Supports placeholders: {name}, {prefix}, {suffix}, {env}, {resourceType}, {resourceAbbr}, {location}.
    /// When null, the resource Name is used as-is.
    /// </summary>
    public NamingTemplate? DefaultNamingTemplate { get; private set; }

    private readonly List<ProjectResourceNamingTemplate> _resourceNamingTemplates = [];

    /// <summary>Gets the project-level per-resource-type naming template overrides.</summary>
    public IReadOnlyCollection<ProjectResourceNamingTemplate> ResourceNamingTemplates
        => _resourceNamingTemplates.AsReadOnly();

    private readonly List<ProjectResourceAbbreviation> _resourceAbbreviations = [];

    /// <summary>Gets the project-level per-resource-type abbreviation overrides.</summary>
    public IReadOnlyCollection<ProjectResourceAbbreviation> ResourceAbbreviations
        => _resourceAbbreviations.AsReadOnly();

    // â”€â”€â”€ Tags â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private readonly List<Tag> _tags = [];

    /// <summary>Gets the project-level default tags applied to all resources.</summary>
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    /// <summary>Replaces all project-level tags with the provided collection.</summary>
    public void SetTags(IEnumerable<Tag> tags)
    {
        _tags.Clear();
        _tags.AddRange(tags);
    }

    // â”€â”€â”€ Pipeline Variable Groups â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private readonly List<ProjectPipelineVariableGroup> _projectPipelineVariableGroups = [];

    /// <summary>Gets the project-level pipeline variable groups (shared across all configurations).</summary>
    public IReadOnlyCollection<ProjectPipelineVariableGroup> PipelineVariableGroups
        => _projectPipelineVariableGroups.AsReadOnly();

    // â”€â”€â”€ Repository Mode â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Gets the layout preset of this project.
    /// </summary>
    public LayoutPreset LayoutPreset { get; private set; } = new(LayoutPresetEnum.MultiRepo);

    // â”€â”€â”€ Project Repositories â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private readonly List<ProjectRepository> _repositories = [];

    /// <summary>Gets the Git repositories declared at project level.</summary>
    public IReadOnlyCollection<ProjectRepository> Repositories => _repositories.AsReadOnly();

    // â”€â”€â”€ Agent Pool â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Gets the name of the self-hosted agent pool to use in generated pipelines.
    /// When <c>null</c>, pipelines use the Microsoft-hosted pool (<c>vmImage: ubuntu-latest</c>).
    /// </summary>
    public string? AgentPoolName { get; private set; }

    // â”€â”€â”€ Constructor â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private Project(ProjectId id, Name name, string? description, UserId ownerId) : base(id)
    {
        Name = name;
        Description = description;
        _members.Add(ProjectMember.CreateOwner(id, ownerId));
    }

    /// <summary>EF Core constructor.</summary>
    private Project() { }

    /// <summary>
    /// Creates a new <see cref="Project"/> with a generated identifier.
    /// The caller is automatically added as Owner.
    /// </summary>
    public static Project Create(Name name, string? description, UserId ownerId)
    {
        var project = new Project(ProjectId.CreateUnique(), name, description, ownerId);
        project.AddDomainEvent(new ProjectCreatedDomainEvent(project.Id));
        return project;
    }

    // â”€â”€â”€ Member Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    // â”€â”€â”€ Environment Definitions Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
        env.SubscriptionId = data.SubscriptionId;
        env.Order = data.Order;
        env.RequiresApproval = data.RequiresApproval;
        env.AzureResourceManagerConnection = data.AzureResourceManagerConnection;
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

    // â”€â”€â”€ Naming Convention Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    // â”€â”€â”€ Resource Abbreviation Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Sets or updates a per-resource-type abbreviation override.</summary>
    public ProjectResourceAbbreviation SetResourceAbbreviation(string resourceType, string abbreviation)
    {
        var existing = _resourceAbbreviations.FirstOrDefault(a => a.ResourceType == resourceType);
        if (existing is not null)
        {
            existing.Update(abbreviation);
            return existing;
        }

        var entry = new ProjectResourceAbbreviation(Id, resourceType, abbreviation);
        _resourceAbbreviations.Add(entry);
        return entry;
    }

    /// <summary>Removes a per-resource-type abbreviation override.</summary>
    public bool RemoveResourceAbbreviation(string resourceType)
    {
        var existing = _resourceAbbreviations.FirstOrDefault(a => a.ResourceType == resourceType);
        if (existing is null)
            return false;
        _resourceAbbreviations.Remove(existing);
        return true;
    }

    // â”€â”€â”€ Pipeline Variable Group Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Adds a new pipeline variable group to this project.</summary>
    public ErrorOr<ProjectPipelineVariableGroup> AddPipelineVariableGroup(string groupName)
    {
        if (_projectPipelineVariableGroups.Any(g =>
                string.Equals(g.GroupName, groupName, StringComparison.OrdinalIgnoreCase)))
        {
            return Domain.Common.Errors.Errors.Project.DuplicateVariableGroupError(groupName);
        }

        var group = ProjectPipelineVariableGroup.Create(Id, groupName);
        _projectPipelineVariableGroups.Add(group);
        return group;
    }

    /// <summary>Removes a pipeline variable group and all its mappings from this project.</summary>
    public ErrorOr<Deleted> RemovePipelineVariableGroup(ProjectPipelineVariableGroupId groupId)
    {
        var group = _projectPipelineVariableGroups.FirstOrDefault(g => g.Id == groupId);
        if (group is null)
            return Domain.Common.Errors.Errors.Project.VariableGroupNotFoundError(groupId);

        _projectPipelineVariableGroups.Remove(group);
        return Result.Deleted;
    }

    // â”€â”€â”€ Project Repositories Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Adds a new <see cref="ProjectRepository"/> to this project.
    /// The current <see cref="LayoutPreset"/> must allow the operation.
    /// Connection details (<paramref name="providerType"/>, <paramref name="repositoryUrl"/>, <paramref name="defaultBranch"/>)
    /// are optional: pass them all to create a fully configured repository, or pass them all as <c>null</c>/empty
    /// to create an unconfigured slot to be completed later.
    /// </summary>
    public ErrorOr<ProjectRepository> AddRepository(
        GitProviderType? providerType,
        string? repositoryUrl,
        string? defaultBranch,
        RepositoryContentKinds contentKinds)
    {
        var allowed = EnsureRepositoryAllowedByLayout(contentKinds, expectedCountAfterAdd: _repositories.Count + 1);
        if (allowed.IsError)
            return allowed.Errors;

        var created = ProjectRepository.Create(Id, providerType, repositoryUrl, defaultBranch, contentKinds);
        if (created.IsError)
            return created.Errors;

        _repositories.Add(created.Value);
        return created.Value;
    }

    /// <summary>Updates an existing <see cref="ProjectRepository"/> by id.</summary>
    public ErrorOr<ProjectRepository> UpdateRepository(
        ProjectRepositoryId id,
        GitProviderType? providerType,
        string? repositoryUrl,
        string? defaultBranch,
        RepositoryContentKinds contentKinds)
    {
        var existing = _repositories.FirstOrDefault(r => r.Id == id);
        if (existing is null)
            return Domain.Common.Errors.Errors.ProjectRepository.NotFound(id);

        // Validate the candidate kinds against the current layout (count remains the same).
        var allowed = EnsureRepositoryAllowedByLayout(contentKinds, expectedCountAfterAdd: _repositories.Count, ignoredId: id);
        if (allowed.IsError)
            return allowed.Errors;

        var updated = existing.Update(providerType, repositoryUrl, defaultBranch, contentKinds);
        if (updated.IsError)
            return updated.Errors;

        return existing;
    }

    /// <summary>Removes a <see cref="ProjectRepository"/> by id.</summary>
    public ErrorOr<Deleted> RemoveRepository(ProjectRepositoryId id)
    {
        var existing = _repositories.FirstOrDefault(r => r.Id == id);
        if (existing is null)
            return Domain.Common.Errors.Errors.ProjectRepository.NotFound(id);

        _repositories.Remove(existing);
        return Result.Deleted;
    }

    // â”€â”€â”€ Repository Mode Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// Sets the layout preset for this project.
    /// Switching to a different preset auto-clears project-level repositories so the user can reconfigure them.
    /// Switching to the current preset is a no-op.
    /// </summary>
    public ErrorOr<Success> SetLayoutPreset(LayoutPreset preset)
    {
        ArgumentNullException.ThrowIfNull(preset);

        if (LayoutPreset.Value == preset.Value)
            return Result.Success;

        _repositories.Clear();

        LayoutPreset = preset;
        return Result.Success;
    }

    private ErrorOr<Success> EnsureRepositoryAllowedByLayout(
        RepositoryContentKinds candidateKinds,
        int expectedCountAfterAdd,
        ProjectRepositoryId? ignoredId = null)
    {
        var others = ignoredId is null
            ? _repositories
            : _repositories.Where(r => r.Id != ignoredId).ToList();

        return LayoutPreset.Value switch
        {
            LayoutPresetEnum.MultiRepo => MultiRepoNotAllowed(),
            LayoutPresetEnum.AllInOne => ValidateAllInOne(candidateKinds, expectedCountAfterAdd),
            LayoutPresetEnum.SplitInfraCode => ValidateSplitInfraCode(candidateKinds, expectedCountAfterAdd, others),
            _ => Result.Success,
        };
    }

    private static ErrorOr<Success> MultiRepoNotAllowed() =>
        Domain.Common.Errors.Errors.Project.RepositoryNotAllowedByLayout(
            LayoutPresetEnum.MultiRepo,
            "in MultiRepo mode the project owns no repository; declare them on each InfrastructureConfig instead.");

    private static ErrorOr<Success> ValidateAllInOne(
        RepositoryContentKinds candidateKinds,
        int expectedCountAfterAdd)
    {
        if (expectedCountAfterAdd > 1)
            return Domain.Common.Errors.Errors.Project.AllInOneRequiresExactlyOneRepository();

        if (!candidateKinds.Has(RepositoryContentKindsEnum.Infrastructure)
            || !candidateKinds.Has(RepositoryContentKindsEnum.ApplicationCode))
            return Domain.Common.Errors.Errors.Project.AllInOneRequiresExactlyOneRepository();

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateSplitInfraCode(
        RepositoryContentKinds candidateKinds,
        int expectedCountAfterAdd,
        IEnumerable<Entities.ProjectRepository> others)
    {
        if (expectedCountAfterAdd > 2)
            return Domain.Common.Errors.Errors.Project.SplitInfraCodeRequiresInfraAndAppRepositories();

        var role = ClassifyRole(candidateKinds);
        if (role is null)
            return Domain.Common.Errors.Errors.Project.SplitInfraCodeRequiresInfraAndAppRepositories();

        var conflict = others.Any(r => RoleConflicts(role.Value, r.ContentKinds));
        if (conflict)
            return Domain.Common.Errors.Errors.Project.SplitInfraCodeRequiresInfraAndAppRepositories();

        return Result.Success;
    }

    private static RepositoryContentKindsEnum? ClassifyRole(RepositoryContentKinds candidateKinds)
    {
        var hasInfra = candidateKinds.Has(RepositoryContentKindsEnum.Infrastructure);
        var hasApp = candidateKinds.Has(RepositoryContentKindsEnum.ApplicationCode);

        if (hasInfra && !hasApp) return RepositoryContentKindsEnum.Infrastructure;
        if (hasApp && !hasInfra) return RepositoryContentKindsEnum.ApplicationCode;
        return null;
    }

    private static bool RoleConflicts(RepositoryContentKindsEnum role, RepositoryContentKinds otherKinds) =>
        otherKinds.Has(role);

    /// <summary>
    /// Returns <see langword="true"/> when a single project-level "generate all" operation is unambiguous,
    /// i.e. the project layout owns its repositories. MultiRepo always returns <see langword="false"/>
    /// because each configuration owns its own repositories.
    /// </summary>
    public bool CanGenerateAllFromProjectLevel()
    {
        return LayoutPreset.Value != LayoutPresetEnum.MultiRepo;
    }

    // â”€â”€â”€ Agent Pool Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Sets or clears the self-hosted agent pool name for pipeline generation.</summary>
    public void SetAgentPoolName(string? poolName)
    {
        AgentPoolName = string.IsNullOrWhiteSpace(poolName) ? null : poolName.Trim();
    }
}
