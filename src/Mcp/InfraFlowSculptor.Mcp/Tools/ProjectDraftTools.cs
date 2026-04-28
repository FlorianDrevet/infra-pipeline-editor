using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Mcp.Common;
using InfraFlowSculptor.Mcp.Drafts;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for creating and validating project creation drafts
/// through a conversational workflow.
/// </summary>
[McpServerToolType]
public sealed class ProjectDraftTools
{
    private ProjectDraftTools() { }

    /// <summary>
    /// Transforms a free-form user prompt into a structured project creation draft.
    /// This tool never creates anything — it only extracts intent, identifies what is missing,
    /// and returns clarification questions when required.
    /// </summary>
    [McpServerTool(Name = "draft_project_from_prompt")]
    [Description("Transforms a free-form user prompt into a structured project creation draft. This tool never creates anything — it only extracts intent, identifies what is missing, and returns clarification questions when required.")]
    public static string DraftProjectFromPrompt(
        IProjectDraftService draftService,
        [Description("The raw user request in natural language.")] string userPrompt)
    {
        var draft = draftService.CreateDraftFromPrompt(userPrompt);
        return SerializeDraft(draft);
    }

    /// <summary>
    /// Validates a project creation draft and returns any remaining missing fields, errors, or warnings.
    /// Call this after enriching a draft with clarification answers.
    /// </summary>
    [McpServerTool(Name = "validate_project_draft")]
    [Description("Validates a project creation draft and returns any remaining missing fields, errors, or warnings. Call this after enriching a draft with clarification answers.")]
    public static string ValidateProjectDraft(
        IProjectDraftService draftService,
        [Description("The draft ID returned by draft_project_from_prompt.")] string draftId,
        [Description("Optional JSON overrides to apply (projectName, layoutPreset, description, etc.).")] string? overrides = null)
    {
        DraftOverrides? parsedOverrides = null;

        if (overrides is not null)
        {
            try
            {
                parsedOverrides = JsonSerializer.Deserialize<DraftOverrides>(overrides, McpJsonDefaults.SerializerOptions);
            }
            catch (JsonException ex)
            {
                return McpJsonDefaults.Error("invalid_overrides", $"Failed to parse overrides: {ex.Message}");
            }
        }

        var draft = draftService.ValidateAndUpdate(draftId, parsedOverrides ?? new DraftOverrides());

        if (draft is null)
        {
            return JsonSerializer.Serialize(
                new { error = "draft_not_found", message = $"No draft with id '{draftId}' was found." },
                McpJsonDefaults.SerializerOptions);
        }

        return SerializeDraft(draft);
    }

    private static string SerializeDraft(ProjectCreationDraft draft)
    {
        return JsonSerializer.Serialize(draft, McpJsonDefaults.SerializerOptions);
    }
}
