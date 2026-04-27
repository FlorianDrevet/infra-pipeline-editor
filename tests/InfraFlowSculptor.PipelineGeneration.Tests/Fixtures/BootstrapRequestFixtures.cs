using InfraFlowSculptor.PipelineGeneration.Models;

namespace InfraFlowSculptor.PipelineGeneration.Tests.Fixtures;

/// <summary>
/// Synthetic, deterministic fixtures for <see cref="BootstrapGenerationRequest"/>.
/// All values are hardcoded to guarantee byte-for-byte stable golden file capture.
/// </summary>
internal static class BootstrapRequestFixtures
{
    private const string Org = "contoso";
    private const string Project = "ifs";
    private const string Repo = "ifs";
    private const string Branch = "main";

    /// <summary>Empty request: no pipelines, environments, or variable groups. Triggers the NoOp job.</summary>
    public static BootstrapGenerationRequest Empty() => new()
    {
        OrganizationName = Org,
        ProjectName = Project,
        RepositoryName = Repo,
        DefaultBranch = Branch,
        Mode = BootstrapMode.FullOwner,
    };

    /// <summary>FullOwner mode with one of each: pipeline, environment, variable group.</summary>
    public static BootstrapGenerationRequest FullOwnerComplete() => new()
    {
        OrganizationName = Org,
        ProjectName = Project,
        RepositoryName = Repo,
        DefaultBranch = Branch,
        Mode = BootstrapMode.FullOwner,
        Pipelines =
        [
            new BootstrapPipelineDefinition(
                Name: "Core - CI",
                YamlPath: ".azuredevops/core/ci.pipeline.yml",
                Folder: "\\Core"),
        ],
        Environments =
        [
            new BootstrapEnvironmentDefinition(
                Name: "ifs-dev",
                DisplayName: "Development",
                RequiresApproval: false),
        ],
        VariableGroups =
        [
            new BootstrapVariableGroupDefinition(
                GroupName: "ifs-core-dev",
                Variables:
                [
                    new BootstrapVariable("LOG_LEVEL", "Information", IsSecret: false),
                    new BootstrapVariable("JWT_SECRET", string.Empty, IsSecret: true),
                ]),
        ],
    };

    /// <summary>FullOwner mode with only pipelines (no environments, no variable groups).</summary>
    public static BootstrapGenerationRequest FullOwnerPipelinesOnly() => new()
    {
        OrganizationName = Org,
        ProjectName = Project,
        RepositoryName = Repo,
        DefaultBranch = Branch,
        Mode = BootstrapMode.FullOwner,
        Pipelines =
        [
            new BootstrapPipelineDefinition(
                Name: "Core - CI",
                YamlPath: ".azuredevops/core/ci.pipeline.yml",
                Folder: "\\Core"),
        ],
    };

    /// <summary>ApplicationOnly mode with one of each: pipeline, environment, variable group. Triggers validation job + dependsOn.</summary>
    public static BootstrapGenerationRequest ApplicationOnlyComplete() => new()
    {
        OrganizationName = Org,
        ProjectName = Project,
        RepositoryName = Repo,
        DefaultBranch = Branch,
        Mode = BootstrapMode.ApplicationOnly,
        Pipelines =
        [
            new BootstrapPipelineDefinition(
                Name: "App - CI",
                YamlPath: ".azuredevops/apps/ci.pipeline.yml",
                Folder: "\\Apps"),
        ],
        Environments =
        [
            new BootstrapEnvironmentDefinition(
                Name: "ifs-dev",
                DisplayName: "Development",
                RequiresApproval: false),
        ],
        VariableGroups =
        [
            new BootstrapVariableGroupDefinition(
                GroupName: "ifs-core-dev",
                Variables:
                [
                    new BootstrapVariable("LOG_LEVEL", "Information", IsSecret: false),
                ]),
        ],
    };

    /// <summary>ApplicationOnly mode with only pipelines (no environments, no variable groups). Skips validation job.</summary>
    public static BootstrapGenerationRequest ApplicationOnlyPipelinesOnly() => new()
    {
        OrganizationName = Org,
        ProjectName = Project,
        RepositoryName = Repo,
        DefaultBranch = Branch,
        Mode = BootstrapMode.ApplicationOnly,
        Pipelines =
        [
            new BootstrapPipelineDefinition(
                Name: "App - CI",
                YamlPath: ".azuredevops/apps/ci.pipeline.yml",
                Folder: "\\Apps"),
        ],
    };
}
