using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;

/// <summary>
/// Canonical default values and repository layout definitions used when creating a new project.
/// Centralises every string literal that maps to a domain concept so that no caller needs to
/// hard-code layout preset names, repository aliases, or content-kind identifiers.
/// </summary>
public static class ProjectSetupDefaults
{
    /// <summary>Default Azure region applied to environments when none is specified.</summary>
    public static string DefaultLocation => Location.DefaultAzureRegionKey;

    /// <summary>Alias of the single repository used in <see cref="LayoutPresetEnum.AllInOne"/> layout.</summary>
    public const string RepoAliasMain = "main";

    /// <summary>Alias of the infrastructure repository in <see cref="LayoutPresetEnum.SplitInfraCode"/> layout.</summary>
    public const string RepoAliasInfra = "infra";

    /// <summary>Alias of the application code repository in <see cref="LayoutPresetEnum.SplitInfraCode"/> layout.</summary>
    public const string RepoAliasApp = "app";

    /// <summary>
    /// Creates the default development environment setup item used when no environment is supplied by the caller.
    /// </summary>
    public static EnvironmentSetupItem CreateDefaultEnvironment() =>
        new(
            Name: "Development",
            ShortName: "dev",
            Prefix: string.Empty,
            Suffix: string.Empty,
            Location: DefaultLocation,
            SubscriptionId: Guid.Empty,
            Order: 0,
            RequiresApproval: false);

    /// <summary>
    /// Builds the canonical default repository slots for the given <paramref name="layoutPreset"/> string value.
    /// </summary>
    /// <remarks>
    /// Returns an empty list for <see cref="LayoutPresetEnum.MultiRepo"/> because in that layout repositories
    /// are declared per infrastructure configuration, not at project level.
    /// Falls back to <see cref="LayoutPresetEnum.AllInOne"/> repositories for any unrecognised value.
    /// </remarks>
    public static IReadOnlyList<RepositorySetupItem> BuildRepositoriesForLayout(string layoutPreset) =>
        layoutPreset switch
        {
            nameof(LayoutPresetEnum.AllInOne) =>
            [
                new RepositorySetupItem(
                    RepoAliasMain,
                    [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)],
                    null, null, null),
            ],
            nameof(LayoutPresetEnum.SplitInfraCode) =>
            [
                new RepositorySetupItem(RepoAliasInfra, [nameof(RepositoryContentKindsEnum.Infrastructure)], null, null, null),
                new RepositorySetupItem(RepoAliasApp, [nameof(RepositoryContentKindsEnum.ApplicationCode)], null, null, null),
            ],
            nameof(LayoutPresetEnum.MultiRepo) => [],
            _ =>
            [
                new RepositorySetupItem(
                    RepoAliasMain,
                    [nameof(RepositoryContentKindsEnum.Infrastructure), nameof(RepositoryContentKindsEnum.ApplicationCode)],
                    null, null, null),
            ],
        };
}
