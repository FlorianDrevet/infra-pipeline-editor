namespace InfraFlowSculptor.Mcp.Drafts;

/// <summary>Manages in-memory project creation drafts for the MCP session.</summary>
public interface IProjectDraftService
{
    /// <summary>Creates a new draft from a free-form user prompt.</summary>
    /// <param name="userPrompt">The raw user request in natural language.</param>
    /// <returns>A new <see cref="ProjectCreationDraft"/> with parsed intent and clarification needs.</returns>
    ProjectCreationDraft CreateDraftFromPrompt(string userPrompt);

    /// <summary>Retrieves a draft by its identifier.</summary>
    /// <param name="draftId">The unique draft identifier.</param>
    /// <returns>The draft if found; otherwise <c>null</c>.</returns>
    ProjectCreationDraft? GetDraft(string draftId);

    /// <summary>Applies overrides to an existing draft, revalidates, and returns it.</summary>
    /// <param name="draftId">The unique draft identifier.</param>
    /// <param name="overrides">Override values to apply.</param>
    /// <returns>The updated draft if found; otherwise <c>null</c>.</returns>
    ProjectCreationDraft? ValidateAndUpdate(string draftId, DraftOverrides overrides);
}
