using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.GenerationCore;
using InfraFlowSculptor.Mcp.Common;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for suggesting optimal infrastructure architecture based on requested resources.
/// </summary>
[McpServerToolType]
public sealed class ArchitectureSuggestionTools
{
    private ArchitectureSuggestionTools() { }

    /// <summary>
    /// Given a list of resource types and constraints, suggests an optimal resource group topology
    /// with dependency wiring and cross-config references.
    /// </summary>
    [McpServerTool(Name = "suggest_architecture")]
    [Description(
        "Given a list of resource types and deployment constraints, suggests an optimal resource group topology. " +
        "Returns recommended resource groups, which resources belong to each, dependency order, " +
        "cross-config references needed, and the tool calls to execute. " +
        "Use this BEFORE creating resources to plan the topology.")]
    public static string SuggestArchitecture(
        [Description("JSON array of resource types to deploy (e.g. [\"ContainerApp\", \"ContainerRegistry\", \"LogAnalyticsWorkspace\"]).")] string resourceTypes,
        [Description("Number of resource groups to use (0 = auto-detect, 1 = single, 2+ = multi-RG).")] int resourceGroupCount = 0,
        [Description("Whether container workloads use Docker/ACR (enables container-specific wiring).")] bool usesDocker = false,
        [Description("Optional: deployment region (e.g. 'FranceCentral').")] string? location = null)
    {
        List<string>? types;
        try
        {
            types = JsonSerializer.Deserialize<List<string>>(resourceTypes, ParseOptions);
        }
        catch (JsonException)
        {
            return McpJsonDefaults.Error("invalid_input", "resourceTypes must be a valid JSON array of strings.");
        }

        if (types is null or { Count: 0 })
        {
            return McpJsonDefaults.Error("empty_input", "At least one resource type is required.");
        }

        var suggestion = BuildSuggestion(types, resourceGroupCount, usesDocker, location);
        return JsonSerializer.Serialize(suggestion, McpJsonDefaults.SerializerOptions);
    }

    private static ArchitectureSuggestion BuildSuggestion(
        List<string> types,
        int rgCount,
        bool usesDocker,
        string? location)
    {
        var normalizedTypes = types
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Classify resources
        var sharedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AzureResourceTypes.ContainerRegistry,
            AzureResourceTypes.LogAnalyticsWorkspace,
            AzureResourceTypes.ApplicationInsights,
            AzureResourceTypes.KeyVault,
            AzureResourceTypes.UserAssignedIdentity,
        };

        var computeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AzureResourceTypes.ContainerApp,
            AzureResourceTypes.ContainerAppEnvironment,
            AzureResourceTypes.WebApp,
            AzureResourceTypes.FunctionApp,
            AzureResourceTypes.AppServicePlan,
        };

        var hasShared = normalizedTypes.Any(t => sharedTypes.Contains(t));
        var hasCompute = normalizedTypes.Any(t => computeTypes.Contains(t));
        var autoDetectedRgCount = (hasShared && hasCompute) ? 2 : 1;
        var effectiveRgCount = rgCount > 0 ? rgCount : autoDetectedRgCount;

        var groups = new List<ResourceGroupSuggestion>();
        var crossConfigRefs = new List<string>();
        var toolCallSequence = new List<string>();

        if (effectiveRgCount >= 2)
        {
            // Shared/common group
            var sharedResources = normalizedTypes.Where(t => sharedTypes.Contains(t)).ToList();
            if (sharedResources.Count > 0)
            {
                groups.Add(new ResourceGroupSuggestion(
                    GroupName: "common",
                    Purpose: "Shared infrastructure (monitoring, registry, identity, secrets)",
                    ResourceTypes: sharedResources,
                    IsShared: true,
                    DeployOrder: 1));
            }

            // App group
            var appResources = normalizedTypes.Where(t => !sharedTypes.Contains(t)).ToList();
            if (appResources.Count > 0)
            {
                groups.Add(new ResourceGroupSuggestion(
                    GroupName: "app",
                    Purpose: "Application compute and data resources",
                    ResourceTypes: appResources,
                    IsShared: false,
                    DeployOrder: 2));
            }

            // Cross-config references
            if (sharedResources.Count > 0 && appResources.Count > 0)
            {
                foreach (var shared in sharedResources)
                {
                    crossConfigRefs.Add($"app-config references {shared} from common-config");
                }
            }

            // Tool call sequence
            toolCallSequence.Add("1. create_infrastructure_config (name: '<project>-common')");
            toolCallSequence.Add("2. create_resource_group (in common config, name: 'rg-<project>-common')");
            toolCallSequence.Add($"3. create_resource (in common RG: {string.Join(", ", sharedResources)})");
            toolCallSequence.Add("4. create_infrastructure_config (name: '<project>-app')");
            toolCallSequence.Add("5. create_resource_group (in app config, name: 'rg-<project>-app')");
            toolCallSequence.Add($"6. create_resource (in app RG: {string.Join(", ", appResources)})");
            toolCallSequence.Add("7. add_cross_config_reference (app config → each shared resource)");
        }
        else
        {
            groups.Add(new ResourceGroupSuggestion(
                GroupName: "main",
                Purpose: "All resources in a single resource group",
                ResourceTypes: normalizedTypes,
                IsShared: false,
                DeployOrder: 1));

            toolCallSequence.Add("1. create_infrastructure_config (name: '<project>-config')");
            toolCallSequence.Add("2. create_resource_group (name: 'rg-<project>')");
            toolCallSequence.Add($"3. create_resource (all: {string.Join(", ", normalizedTypes)})");
        }

        // Docker-specific wiring
        var dockerWiring = new List<string>();
        if (usesDocker || normalizedTypes.Contains(AzureResourceTypes.ContainerRegistry, StringComparer.OrdinalIgnoreCase))
        {
            dockerWiring.Add("Create UserAssignedIdentity if not already present");
            dockerWiring.Add("add_role_assignment: UAI → ContainerRegistry with role 'AcrPull' (7f951dda-4ed3-4680-a7ca-43fe172d538d)");
            dockerWiring.Add("Set each ContainerApp's acrPullIdentityId to the UAI resource ID");
            dockerWiring.Add("Set each ContainerApp's containerRegistryId to the ACR resource ID");
            dockerWiring.Add("Configure dockerImageName and dockerfilePath for each ContainerApp");

            if (!toolCallSequence.Any(s => s.Contains("add_role_assignment")))
            {
                toolCallSequence.Add($"{toolCallSequence.Count + 1}. add_role_assignment (UAI → ACR, role: AcrPull)");
                toolCallSequence.Add($"{toolCallSequence.Count + 1}. set_resource_environment_settings (each ContainerApp)");
            }
        }

        // Standard post-creation steps
        toolCallSequence.Add($"{toolCallSequence.Count + 1}. set_project_naming_template");
        toolCallSequence.Add($"{toolCallSequence.Count + 1}. set_resource_environment_settings (per resource, per env)");
        toolCallSequence.Add($"{toolCallSequence.Count + 1}. generate_project_bicep");

        return new ArchitectureSuggestion(
            RecommendedResourceGroupCount: effectiveRgCount,
            ResourceGroups: groups,
            CrossConfigReferences: crossConfigRefs,
            DockerWiring: dockerWiring,
            ToolCallSequence: toolCallSequence,
            Location: location ?? "FranceCentral",
            Notes:
            [
                effectiveRgCount >= 2
                    ? "Multi-RG topology separates lifecycle: shared infra changes independently from app resources."
                    : "Single RG is simpler but all resources share the same deployment lifecycle.",
                usesDocker
                    ? "Docker workflow requires ACR + UAI + AcrPull role assignment before ContainerApp can pull images."
                    : "No Docker/ACR wiring needed.",
            ]);
    }

    private static readonly JsonSerializerOptions ParseOptions = new() { PropertyNameCaseInsensitive = true };

    private sealed record ArchitectureSuggestion(
        int RecommendedResourceGroupCount,
        List<ResourceGroupSuggestion> ResourceGroups,
        List<string> CrossConfigReferences,
        List<string> DockerWiring,
        List<string> ToolCallSequence,
        string Location,
        List<string> Notes);

    private sealed record ResourceGroupSuggestion(
        string GroupName,
        string Purpose,
        List<string> ResourceTypes,
        bool IsShared,
        int DeployOrder);
}
