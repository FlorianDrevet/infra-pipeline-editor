using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Prompts;

/// <summary>
/// Provides MCP prompts that guide the AI agent through the project creation workflow.
/// </summary>
[McpServerPromptType]
public sealed class ProjectCreationPrompts
{
    /// <summary>
    /// Returns instructional guidance for creating a new Infra Flow Sculptor project through natural language.
    /// </summary>
    [McpServerPrompt(Name = "project_creation_guide")]
    [Description("Provides guidance for creating a new Infra Flow Sculptor project through natural language.")]
    public static ChatMessage GetProjectCreationGuide()
    {
        return new ChatMessage(
            ChatRole.User,
            """
            You are helping a user create a new infrastructure project in Infra Flow Sculptor.

            ## Workflow
            1. Call `list_repository_topologies` and `list_supported_resource_types` to discover available options.
            2. Call `draft_project_from_prompt` with the user's request.
            3. If the draft status is `RequiresClarification`, present the clarification questions to the user and collect answers.
            4. Call `validate_project_draft` with the answers as overrides.
            5. Repeat steps 3-4 until status is `ReadyToCreate`.
            6. Show the user a summary of what will be created and ask for confirmation.
            7. Call `create_project_from_draft` to create the project.

            ## Rules
            - NEVER guess the repository topology (AllInOne, SplitInfraCode, MultiRepo) — always ask.
            - NEVER create a project without explicit user confirmation.
            - A project name is always required.
            - At least one environment is required (Development is the default).
            - Subscription IDs and locations can be defaulted but should be flagged as warnings.
            """);
    }
}
