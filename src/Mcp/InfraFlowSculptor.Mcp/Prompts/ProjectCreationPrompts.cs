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
            - When multiple instances of the same type are needed, each MUST have a distinct semantic name.

            ## Multi-Resource-Group Projects
            When the user mentions "common/shared resources" separate from "application resources",
            or mentions different resource groups (RGs):

            1. Call `suggest_architecture` with the resource types and constraints to get the recommended topology.
            2. After `create_project_from_draft`, create the infrastructure as follows:
               a. Call `create_infrastructure_config` for each deployment unit (e.g. "common", "app")
               b. Call `create_resource_group` under each config with appropriate name and location
               c. Create shared resources (ACR, LAW, AppInsights, KeyVault) in the common/shared RG
               d. Create app resources (ContainerApps, CAE) in the app RG
               e. Call `add_cross_config_reference` so the app config can reference shared resources
            3. Use `get_project_structure` to verify the topology before proceeding to configuration.

            ## Containerized Workloads (ACA + ACR + Docker)
            When Container Apps + Container Registry are requested:

            1. Create ContainerRegistry first (in the common/shared RG if multi-RG)
            2. Create UserAssignedIdentity for AcrPull
            3. Create ContainerAppEnvironment (requires a LogAnalyticsWorkspace reference)
            4. Create each ContainerApp with `containerAppEnvironmentId` + `containerRegistryId`
            5. Call `list_available_role_definitions` with category 'container' to get the AcrPull role GUID
            6. Call `add_role_assignment` for each ContainerApp:
               - sourceResourceId = UserAssignedIdentity ID
               - targetResourceId = ContainerRegistry ID
               - roleDefinitionId = AcrPull GUID (7f951dda-4ed3-4680-a7ca-43fe172d538d)
               - managedIdentityType = "UserAssigned"
               - userAssignedIdentityId = UAI resource ID
            7. Call `link_container_app_to_acr` as a shortcut for steps 5-6 if available
            8. Configure per-env settings: cpuCores, memoryGi, minReplicas, maxReplicas, ingressTargetPort

            ## Post-Creation Setup (MANDATORY after project creation)
            After `create_project_from_draft` succeeds, perform these steps:

            ### Step 0: Multi-RG Setup (if applicable)
            If the draft detected multi-resource-group topology (check `resourceGroupAssignments` in draft):
            - Follow the "Multi-Resource-Group Projects" section above
            - Use `get_project_structure` after setup to confirm topology

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
            Use `add_keyvault_secret_app_setting` for secrets that should come from Key Vault.

            ### Step E: Generate Bicep
            Call `generate_project_bicep` to produce the final infrastructure-as-code output.

            ### Step F: Verify
            Call `get_project_structure` to show the user the final state of the project.
            Call `list_project_resources` if you need specific resource IDs for any remaining configuration.

            ## Summary of Available Tools

            ### Discovery & Planning
            - `list_repository_topologies` — Available repository layouts
            - `list_supported_resource_types` — All supported Azure resource types
            - `suggest_architecture` — Get recommended RG topology for a set of resources
            - `list_available_role_definitions` — RBAC roles with GUIDs for role assignments

            ### Project Lifecycle
            - `draft_project_from_prompt` — Parse user intent into a draft
            - `validate_project_draft` — Validate/enrich a draft with overrides
            - `create_project_from_draft` — Create the project from a validated draft

            ### Infrastructure Setup
            - `create_infrastructure_config` — Create a deployment unit within a project
            - `create_resource_group` — Create a resource group within a config
            - `create_resource` — Create resources in a resource group
            - `add_cross_config_reference` — Wire a shared resource across configs

            ### Resource Configuration
            - `set_project_naming_template` — Set the default naming pattern
            - `set_project_resource_naming_template` — Override naming for a specific resource type
            - `remove_project_resource_naming_template` — Revert to default naming for a resource type
            - `set_project_resource_abbreviation` — Set/override the {resourceAbbr} value for a type
            - `remove_project_resource_abbreviation` — Revert to system default abbreviation
            - `set_resource_environment_settings` — Configure per-env settings (SKU, capacity, replicas, etc.)

            ### App Settings & Wiring
            - `add_app_setting` — Add a static environment variable with per-env values
            - `add_output_reference_app_setting` — Wire a resource output as an env var
            - `add_keyvault_secret_app_setting` — Wire a Key Vault secret as an env var
            - `list_app_settings` — View existing app settings on a resource
            - `remove_app_setting` — Delete an app setting
            - `add_role_assignment` — Define RBAC access between resources

            ### Container Shortcuts
            - `link_container_app_to_acr` — One-call setup: ACR + UAI + AcrPull role assignment

            ### Querying & Verification
            - `get_project_structure` — Full hierarchical view (configs → RGs → resources)
            - `list_project_resources` — Flat list of all resources with IDs
            - `generate_project_bicep` — Generate the final Bicep IaC files
            """);
    }
}
