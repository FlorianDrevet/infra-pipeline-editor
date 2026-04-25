using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Projects.Common;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;

/// <summary>
/// Atomic command that creates a new project together with its initial layout, environments
/// and (optionally) project-level repositories. All entities are persisted in a single
/// Unit of Work: if any step fails the project is not created.
/// </summary>
/// <param name="Name">Name of the project to create.</param>
/// <param name="Description">Optional description.</param>
/// <param name="LayoutPreset">Layout preset (<c>AllInOne</c>, <c>SplitInfraCode</c> or <c>MultiRepo</c>).</param>
/// <param name="Environments">At least one environment definition (the wizard always creates one).</param>
/// <param name="Repositories">
/// Optional list of project-level repository slots. Required for <c>AllInOne</c> (exactly 1) and
/// <c>SplitInfraCode</c> (exactly 2). Must be empty for <c>MultiRepo</c>.
/// Connection details inside each entry are optional and may be filled later.
/// </param>
public record CreateProjectWithSetupCommand(
    string Name,
    string? Description,
    string LayoutPreset,
    IReadOnlyList<EnvironmentSetupItem> Environments,
    IReadOnlyList<RepositorySetupItem> Repositories
) : ICommand<ProjectResult>;

/// <summary>One environment definition inside a <see cref="CreateProjectWithSetupCommand"/>.</summary>
/// <param name="Name">Display name (e.g. <c>Development</c>).</param>
/// <param name="ShortName">Short identifier (e.g. <c>dev</c>).</param>
/// <param name="Prefix">Resource name prefix (may be empty).</param>
/// <param name="Suffix">Resource name suffix (may be empty).</param>
/// <param name="Location">Azure region key (e.g. <c>WestEurope</c>).</param>
/// <param name="SubscriptionId">Azure subscription id; <see cref="System.Guid.Empty"/> means "to configure later".</param>
/// <param name="Order">Deployment order (0-based).</param>
/// <param name="RequiresApproval">Whether this environment requires a deployment approval.</param>
public record EnvironmentSetupItem(
    string Name,
    string ShortName,
    string Prefix,
    string Suffix,
    string Location,
    Guid SubscriptionId,
    int Order,
    bool RequiresApproval);

/// <summary>One project-level repository slot inside a <see cref="CreateProjectWithSetupCommand"/>.</summary>
/// <param name="Alias">Project-scoped slug (lowercase, digits, hyphens).</param>
/// <param name="ContentKinds">Repository content kinds (e.g. <c>Infrastructure</c>, <c>ApplicationCode</c>).</param>
/// <param name="ProviderType">Optional provider (<c>GitHub</c>/<c>AzureDevOps</c>) — fill later if null.</param>
/// <param name="RepositoryUrl">Optional repository URL — fill later if null/empty.</param>
/// <param name="DefaultBranch">Optional default branch — fill later if null/empty.</param>
public record RepositorySetupItem(
    string Alias,
    IReadOnlyList<string> ContentKinds,
    string? ProviderType,
    string? RepositoryUrl,
    string? DefaultBranch);
