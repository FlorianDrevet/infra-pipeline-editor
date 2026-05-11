using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.Projects.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Domain.UserAggregate.ValueObjects;
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
    private const string InvalidLayoutPresetCode = "LayoutPreset.Invalid";
    private const string InvalidLocationCode = "Location.Invalid";

    /// <summary>Per-resource-type naming template overrides applied on project creation.</summary>
    private static readonly Dictionary<string, string> DefaultResourceTemplates = new()
    {
        ["ResourceGroup"] = "{resourceAbbr}-{name}{suffix}",
        ["StorageAccount"] = "{name}{resourceAbbr}{envShort}",
    };

    /// <inheritdoc />
    public async Task<ErrorOr<ProjectResult>> Handle(
        CreateProjectWithSetupCommand command, CancellationToken cancellationToken)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        var project = CreateProject(command, userId);

        var layoutResult = ApplyLayoutPreset(project, command.LayoutPreset);
        if (layoutResult.IsError)
            return layoutResult.Errors;

        var environmentResult = AddEnvironments(project, command.Environments);
        if (environmentResult.IsError)
            return environmentResult.Errors;

        var repositoryResult = AddRepositories(project, command.Repositories);
        if (repositoryResult.IsError)
            return repositoryResult.Errors;

        var saved = await repository.AddAsync(project);
        return ProjectResultMapper.ToProjectResult(saved);
    }

    private static Project CreateProject(CreateProjectWithSetupCommand command, UserId userId)
    {
        var project = Project.Create(new Name(command.Name), command.Description, userId);
        ApplyDefaultNamingTemplates(project);
        return project;
    }

    private static void ApplyDefaultNamingTemplates(Project project)
    {
        project.SetDefaultNamingTemplate(new NamingTemplate(DefaultTemplate));

        foreach (var (resourceType, template) in DefaultResourceTemplates)
            project.SetResourceNamingTemplate(resourceType, new NamingTemplate(template));
    }

    private static ErrorOr<Success> ApplyLayoutPreset(Project project, string layoutPreset)
    {
        if (!Enum.TryParse<LayoutPresetEnum>(layoutPreset, ignoreCase: true, out var layoutEnum))
            return Error.Validation(InvalidLayoutPresetCode, $"Invalid layout preset '{layoutPreset}'.");

        return project.SetLayoutPreset(new LayoutPreset(layoutEnum));
    }

    private static ErrorOr<Success> AddEnvironments(Project project, IReadOnlyList<EnvironmentSetupItem> environments)
    {
        foreach (var environmentItem in environments)
        {
            var environmentDataResult = CreateEnvironmentData(environmentItem);
            if (environmentDataResult.IsError)
                return environmentDataResult.Errors;

            project.AddEnvironment(environmentDataResult.Value);
        }

        return Result.Success;
    }

    private static ErrorOr<EnvironmentDefinitionData> CreateEnvironmentData(EnvironmentSetupItem environmentItem)
    {
        if (!Enum.TryParse<Location.LocationEnum>(environmentItem.Location, ignoreCase: true, out var locationEnum))
            return Error.Validation(InvalidLocationCode, $"Invalid location '{environmentItem.Location}'.");

        return new EnvironmentDefinitionData(
            new Name(environmentItem.Name),
            new ShortName(environmentItem.ShortName),
            new Prefix(environmentItem.Prefix ?? string.Empty),
            new Suffix(environmentItem.Suffix ?? string.Empty),
            new Location(locationEnum),
            new SubscriptionId(environmentItem.SubscriptionId),
            new Order(environmentItem.Order),
            new RequiresApproval(environmentItem.RequiresApproval),
            AzureResourceManagerConnection: null,
            Tags: []);
    }

    private static ErrorOr<Success> AddRepositories(Project project, IReadOnlyList<RepositorySetupItem> repositories)
    {
        foreach (var repositoryItem in repositories)
        {
            var addRepositoryResult = AddRepository(project, repositoryItem);
            if (addRepositoryResult.IsError)
                return addRepositoryResult.Errors;
        }

        return Result.Success;
    }

    private static ErrorOr<Success> AddRepository(Project project, RepositorySetupItem repositoryItem)
    {
        var providerTypeResult = TryParseProviderType(repositoryItem.ProviderType, out var providerType);
        if (providerTypeResult.IsError)
            return providerTypeResult.Errors;

        var aliasResult = RepositoryAlias.Create(repositoryItem.Alias);
        if (aliasResult.IsError)
            return aliasResult.Errors;

        var contentKindsResult = ParseContentKinds(repositoryItem.ContentKinds);
        if (contentKindsResult.IsError)
            return contentKindsResult.Errors;

        var addResult = project.AddRepository(
            aliasResult.Value,
            providerType,
            repositoryItem.RepositoryUrl,
            repositoryItem.DefaultBranch,
            contentKindsResult.Value);
        if (addResult.IsError)
            return addResult.Errors;

        return Result.Success;
    }

    private static ErrorOr<Success> TryParseProviderType(string? providerTypeValue, out GitProviderType? providerType)
    {
        providerType = null;

        if (string.IsNullOrWhiteSpace(providerTypeValue))
            return Result.Success;

        if (!Enum.TryParse<GitProviderTypeEnum>(providerTypeValue, ignoreCase: true, out var providerEnum))
            return Errors.GitRepository.InvalidProviderType(providerTypeValue);

        providerType = new GitProviderType(providerEnum);
        return Result.Success;
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
