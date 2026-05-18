using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectEnvironment;
using InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for managing project-level configuration: environments and repositories.
/// </summary>
[McpServerToolType]
public sealed class ProjectManagementTools
{
    private const string InvalidProjectIdError = "invalid_project_id";

    private ProjectManagementTools() { }

    /// <summary>
    /// Adds an environment to a project with its deployment settings.
    /// </summary>
    [McpServerTool(Name = "add_project_environment")]
    [Description(
        "Adds an environment (e.g. Development, Staging, Production) to a project. " +
        "Provide shortName, prefix, suffix, location, subscriptionId, and optional tags as a JSON object {\"key\": \"value\"}.")]
    public static async Task<string> AddProjectEnvironment(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("Environment display name (e.g. 'Development', 'Production').")] string name,
        [Description("Short name used in naming templates (e.g. 'dev', 'prod').")] string shortName,
        [Description("Prefix added to resource names for this environment (e.g. 'dev-').")] string prefix,
        [Description("Suffix added to resource names for this environment (e.g. '-dev').")] string suffix,
        [Description("Azure region (e.g. 'FranceCentral', 'WestEurope').")] string location,
        [Description("Azure subscription ID (GUID).")] string subscriptionId,
        [Description("Deployment order (0-based).")] int order = 0,
        [Description("Whether deployment requires manual approval.")] bool requiresApproval = false,
        [Description("Azure Resource Manager service connection name.")] string? azureResourceManagerConnection = null,
        [Description("Optional JSON object of tags: {\"environment\": \"dev\", \"team\": \"infra\"}.")] string? tags = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(projectId, out var id))
        {
            return McpJsonDefaults.Error(InvalidProjectIdError, "The projectId must be a valid GUID.");
        }

        if (!Guid.TryParse(subscriptionId, out var subId))
        {
            return McpJsonDefaults.Error("invalid_subscription_id", "The subscriptionId must be a valid GUID.");
        }

        var tagList = ParseTags(tags);

        var command = new AddProjectEnvironmentCommand(
            ProjectId: new ProjectId(id),
            Name: name,
            ShortName: shortName,
            Prefix: prefix,
            Suffix: suffix,
            Location: location,
            SubscriptionId: subId,
            Order: order,
            RequiresApproval: requiresApproval,
            AzureResourceManagerConnection: azureResourceManagerConnection,
            Tags: tagList);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            env => JsonSerializer.Serialize(new
            {
                status = "success",
                environmentId = env.Id.ToString(),
                name = env.Name,
                shortName = env.ShortName,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Configures a git repository on a project.
    /// </summary>
    [McpServerTool(Name = "set_git_configuration")]
    [Description(
        "Adds a git repository configuration to a project. " +
        "Specify the provider type (AzureDevOps, GitHub), repository URL, default branch, and content kinds.")]
    public static async Task<string> SetGitConfiguration(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("Repository alias (e.g. 'main', 'infra').")] string alias,
        [Description("Git provider type: 'AzureDevOps' or 'GitHub'.")] string? providerType = null,
        [Description("Full repository URL.")] string? repositoryUrl = null,
        [Description("Default branch name (e.g. 'main').")] string? defaultBranch = null,
        [Description("JSON array of content kinds: [\"Infrastructure\", \"ApplicationCode\"].")] string? contentKinds = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(projectId, out var id))
        {
            return McpJsonDefaults.Error(InvalidProjectIdError, "The projectId must be a valid GUID.");
        }

        var kinds = ParseContentKinds(contentKinds);

        var command = new AddProjectRepositoryCommand(
            ProjectId: new ProjectId(id),
            Alias: alias,
            ProviderType: providerType,
            RepositoryUrl: repositoryUrl,
            DefaultBranch: defaultBranch,
            ContentKinds: kinds);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            repoId => JsonSerializer.Serialize(new
            {
                status = "success",
                repositoryId = repoId.Value.ToString(),
                alias,
                providerType,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static IReadOnlyList<(string Name, string Value)> ParseTags(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
        {
            return [];
        }

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(tagsJson);
            return dict?.Select(kvp => (kvp.Key, kvp.Value)).ToList() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IReadOnlyList<string> ParseContentKinds(string? contentKindsJson)
    {
        if (string.IsNullOrWhiteSpace(contentKindsJson))
        {
            return ["Infrastructure", "ApplicationCode"];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(contentKindsJson) ?? ["Infrastructure", "ApplicationCode"];
        }
        catch (JsonException)
        {
            return ["Infrastructure", "ApplicationCode"];
        }
    }
}
