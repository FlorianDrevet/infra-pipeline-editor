using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;
using InfraFlowSculptor.Mcp.Drafts;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides the MCP tool that creates a project from a completed draft
/// by dispatching a <see cref="CreateProjectWithSetupCommand"/> via MediatR.
/// </summary>
[McpServerToolType]
public sealed class ProjectCreationTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Creates a project in Infra Flow Sculptor from a completed and validated draft.
    /// The draft must have status <see cref="DraftStatus.ReadyToCreate"/>.
    /// </summary>
    [McpServerTool(Name = "create_project_from_draft")]
    [Description("Creates a project in Infra Flow Sculptor from a completed and validated draft. The draft must have status 'ReadyToCreate'.")]
    public static async Task<string> CreateProjectFromDraft(
        IProjectDraftService draftService,
        ISender mediator,
        [Description("The draft ID of the completed draft.")] string draftId)
    {
        var draft = draftService.GetDraft(draftId);
        if (draft is null)
        {
            return JsonError("draft_not_found", $"No draft with id '{draftId}' was found.");
        }

        if (draft.Status != DraftStatus.ReadyToCreate)
        {
            return JsonError("draft_not_ready", "The draft is not ready for creation. Call validate_project_draft first.");
        }

        var command = BuildCommand(draft.Intent);
        var result = await mediator.Send(command);

        if (result.IsError)
        {
            return JsonError("creation_failed", string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return JsonSerializer.Serialize(new
        {
            status = "created",
            projectId = result.Value.Id.Value.ToString(),
            projectName = result.Value.Name.Value,
        }, JsonOptions);
    }

    private static CreateProjectWithSetupCommand BuildCommand(DraftProjectIntent intent)
    {
        var environments = intent.Environments?.Select(e => new EnvironmentSetupItem(
            e.Name,
            e.ShortName,
            e.Prefix,
            e.Suffix,
            e.Location,
            e.SubscriptionId,
            e.Order,
            e.RequiresApproval
        )).ToList() ?? [];

        var repositories = intent.Repositories?.Select(r => new RepositorySetupItem(
            r.Alias,
            r.ContentKinds,
            r.ProviderType,
            r.RepositoryUrl,
            r.DefaultBranch
        )).ToList() ?? [];

        return new CreateProjectWithSetupCommand(
            Name: intent.ProjectName!,
            Description: intent.Description,
            LayoutPreset: intent.LayoutPreset!,
            Environments: environments,
            Repositories: repositories
        );
    }

    private static string JsonError(string error, string message)
    {
        return JsonSerializer.Serialize(new { error, message }, JsonOptions);
    }
}
