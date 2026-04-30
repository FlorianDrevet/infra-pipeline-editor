using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using Location = InfraFlowSculptor.Domain.Common.ValueObjects.Location;
using Name = InfraFlowSculptor.Domain.Common.ValueObjects.Name;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;

/// <summary>Handles the <see cref="CreateProjectWithSetupCommand"/>.</summary>
/// <remarks>
/// Creates a project, applies default naming templates, sets the layout preset,
/// then adds all environments and repositories declared by the wizard.
/// All persistence happens in a single Unit of Work via the pipeline behaviour.
/// </remarks>
public sealed class CreateProjectWithSetupCommandHandler(
    IProjectRepository repository,
    ICurrentUser currentUser)
    : ICommandHandler<CreateProjectWithSetupCommand, ProjectResult>
{
    /// <summary>Default naming template applied to every new project.</summary>
    private const string DefaultTemplate = "{name}-{resourceAbbr}{suffix}";

    /// <summary>Per-resource-type naming template overrides applied on project creation.</summary>
    private static readonly Dictionary<string, string> DefaultResourceTemplates = new()
    {
        ["ResourceGroup"] = "{resourceAbbr}-{name}{suffix}",
        ["StorageAccount"] = "{name}{resourceAbbr}{envShort}",
    };

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Tracked under test-debt #22: refactoring deferred until dedicated unit-test coverage protects against behavioural regressions. The method orchestrates a single coherent business operation and would lose readability without proper test guards.")]
    public async Task<ErrorOr<ProjectResult>> Handle(
        CreateProjectWithSetupCommand command, CancellationToken cancellationToken)
    {
        var nameVo = new Name(command.Name);
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        var project = Project.Create(nameVo, command.Description, userId);

        // Naming defaults.
        project.SetDefaultNamingTemplate(new NamingTemplate(DefaultTemplate));
        foreach (var (resourceType, template) in DefaultResourceTemplates)
            project.SetResourceNamingTemplate(resourceType, new NamingTemplate(template));

        // Layout preset (default in ctor is MultiRepo; switch only if different).
        if (!Enum.TryParse<LayoutPresetEnum>(command.LayoutPreset, ignoreCase: true, out var layoutEnum))
            return Error.Validation("LayoutPreset.Invalid", $"Invalid layout preset '{command.LayoutPreset}'.");

        var layoutResult = project.SetLayoutPreset(new LayoutPreset(layoutEnum));
        if (layoutResult.IsError)
            return layoutResult.Errors;

        // Environments.
        foreach (var envItem in command.Environments)
        {
            if (!Enum.TryParse<Location.LocationEnum>(envItem.Location, ignoreCase: true, out var locationEnum))
                return Error.Validation("Location.Invalid", $"Invalid location '{envItem.Location}'.");

            var data = new EnvironmentDefinitionData(
                new Name(envItem.Name),
                new ShortName(envItem.ShortName),
                new Prefix(envItem.Prefix ?? string.Empty),
                new Suffix(envItem.Suffix ?? string.Empty),
                new Location(locationEnum),
                new SubscriptionId(envItem.SubscriptionId),
                new Order(envItem.Order),
                new RequiresApproval(envItem.RequiresApproval),
                AzureResourceManagerConnection: null,
                Tags: []);

            project.AddEnvironment(data);
        }

        // Repositories (optional, layout-dependent).
        foreach (var repoItem in command.Repositories)
        {
            GitProviderType? providerType = null;
            if (!string.IsNullOrWhiteSpace(repoItem.ProviderType))
            {
                if (!Enum.TryParse<GitProviderTypeEnum>(repoItem.ProviderType, ignoreCase: true, out var providerEnum))
                    return Errors.GitRepository.InvalidProviderType(repoItem.ProviderType);
                providerType = new GitProviderType(providerEnum);
            }

            var aliasResult = RepositoryAlias.Create(repoItem.Alias);
            if (aliasResult.IsError)
                return aliasResult.Errors;

            var contentKindsResult = ParseContentKinds(repoItem.ContentKinds);
            if (contentKindsResult.IsError)
                return contentKindsResult.Errors;

            var addResult = project.AddRepository(
                aliasResult.Value,
                providerType,
                repoItem.RepositoryUrl,
                repoItem.DefaultBranch,
                contentKindsResult.Value);
            if (addResult.IsError)
                return addResult.Errors;
        }

        var saved = await repository.AddAsync(project);
        return ProjectResultMapper.ToProjectResult(saved);
    }

    private static ErrorOr<RepositoryContentKinds> ParseContentKinds(IReadOnlyList<string> kinds)
    {
        var flags = RepositoryContentKindsEnum.None;
        foreach (var raw in kinds)
        {
            if (!Enum.TryParse<RepositoryContentKindsEnum>(raw, ignoreCase: true, out var parsed)
                || parsed == RepositoryContentKindsEnum.None)
            {
                return Errors.ProjectRepository.NoContentKind();
            }

            flags |= parsed;
        }

        return RepositoryContentKinds.Create(flags);
    }
}
