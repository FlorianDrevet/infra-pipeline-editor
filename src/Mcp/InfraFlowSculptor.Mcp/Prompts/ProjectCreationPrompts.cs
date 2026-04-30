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
    private ProjectCreationPrompts() { }
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
            8. After creation, proceed to **Post-Creation Setup** (see below).

            ## Rules
            - NEVER guess the repository topology (AllInOne, SplitInfraCode, MultiRepo) — always ask.
            - NEVER create a project without explicit user confirmation.
            - A project name is always required.
            - At least one environment is required (Development is the default).
            - Subscription IDs are optional when creating environments. Missing values must never block draft validation or project creation; keep them as warnings and configure them later.
            - Locations can be defaulted but should be flagged as warnings.
            - For AllInOne or SplitInfraCode topologies, always ask for the repository URL. Pass it via `repositoryUrl` in overrides.
            - Resource names should be SHORT semantic identifiers (e.g. 'api', 'frontend', 'worker') — the naming template system adds the project name, abbreviation, and environment prefix/suffix automatically.
            - When a SqlServer is requested, always include a SqlDatabase as well (the system does this automatically).

            ## Post-Creation Setup (MANDATORY after project creation)
            After `create_project_from_draft` succeeds, perform these steps:

            ### Step A: Configure Naming Template
            Call `set_project_naming_template` to define how resource names are generated in Bicep.
            Example: `{projectName}-{resourceAbbr}-{envSuffix}`
            Available placeholders: {projectName}, {resourceName}, {resourceAbbr}, {envPrefix}, {envSuffix}, {location}.

            ### Step B: Configure Resource Abbreviations (if defaults are not suitable)
            Call `set_project_resource_abbreviation` for each resource type if the default abbreviation needs customization.
            Examples: 'kv' for KeyVault, 'acr' for ContainerRegistry, 'sql' for SqlServer, 'ca' for ContainerApp.

            ### Step C: Configure Per-Environment Settings
            For each created resource, call `set_resource_environment_settings` with the appropriate settings per environment.
            - KeyVault: sku (Standard/Premium)
            - ContainerApp: cpuCores, memoryGi, minReplicas, maxReplicas, ingressEnabled, ingressTargetPort, ingressExternal
            - StorageAccount: sku (Standard_LRS, Standard_GRS, etc.)
            - SqlDatabase: sku (Basic, Standard, Premium), maxSizeGb, zoneRedundant
            - ContainerRegistry: sku (Basic, Standard, Premium)
            - AppServicePlan: sku (B1, S1, P1v3, etc.), capacity

            ### Step D: Configure App Settings / Environment Variables (for compute resources)
            For ContainerApp, WebApp, and FunctionApp, call `add_app_setting` to add environment variables.
            Use `add_output_reference_app_setting` to wire outputs from one resource to another (e.g., connection strings).

            ### Step E: Generate Bicep
            Call `generate_project_bicep` to produce the final infrastructure-as-code output.

            ## Summary of Available Post-Creation Tools
            - `set_project_naming_template` — Set the default naming pattern
            - `set_project_resource_naming_template` — Override naming for a specific resource type
            - `remove_project_resource_naming_template` — Revert to default naming for a resource type
            - `set_project_resource_abbreviation` — Set/override the {resourceAbbr} value for a type
            - `remove_project_resource_abbreviation` — Revert to system default abbreviation
            - `set_resource_environment_settings` — Configure per-env settings (SKU, capacity, replicas, etc.)
            - `add_app_setting` — Add a static environment variable with per-env values
            - `add_output_reference_app_setting` — Wire a resource output as an env var
            - `list_app_settings` — View existing app settings on a resource
            - `remove_app_setting` — Delete an app setting
            """);
    }
}
