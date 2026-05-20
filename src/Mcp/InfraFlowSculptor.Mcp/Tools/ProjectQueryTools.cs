using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Application.Projects.Queries.ListProjectConfigs;
using InfraFlowSculptor.Application.ResourceGroups.Queries.ListResourceGroupsByConfig;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for querying project structure (configs, resource groups, resources).
/// These give the agent a full picture of what has been created.
/// </summary>
[McpServerToolType]
public sealed class ProjectQueryTools
{
    private ProjectQueryTools() { }

    /// <summary>
    /// Returns the full hierarchical structure of a project: configs → resource groups → resources.
    /// </summary>
    [McpServerTool(Name = "get_project_structure")]
    [Description(
        "Returns the full hierarchical structure of a project: infrastructure configs, their resource groups, " +
        "and all resources within each group. Use this to understand what has been created and plan next steps.")]
    public static async Task<string> GetProjectStructure(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(projectId, out var guid))
        {
            return McpJsonDefaults.Error("invalid_project_id", $"'{projectId}' is not a valid GUID.");
        }

        var pid = new ProjectId(guid);

        var projectResult = await mediator.Send(new GetProjectQuery(pid), cancellationToken);
        if (projectResult.IsError)
        {
            return McpJsonDefaults.Error("project_not_found", string.Join("; ", projectResult.Errors.Select(e => e.Description)));
        }

        var project = projectResult.Value;

        var configsResult = await mediator.Send(new ListProjectConfigsQuery(pid), cancellationToken);
        if (configsResult.IsError)
        {
            return McpJsonDefaults.Error("configs_error", string.Join("; ", configsResult.Errors.Select(e => e.Description)));
        }

        var configsWithGroups = new List<ProjectStructureConfigResponse>();

        foreach (var config in configsResult.Value)
        {
            var rgResult = await mediator.Send(new ListResourceGroupsByConfigQuery(config.Id), cancellationToken);

            IReadOnlyList<ProjectResourceGroupResponse> resourceGroups = rgResult.IsError
                ? []
                : rgResult.Value.Select(rg => new ProjectResourceGroupResponse(
                    rg.Id.Value.ToString(),
                    rg.Name.Value,
                    rg.Location.Value.ToString(),
                    rg.Resources.Select(r => new ProjectResourceSummaryResponse(
                        r.Id.Value.ToString(),
                        r.ResourceType.Value,
                        r.Name.Value)).ToArray(),
                    rg.Resources.Length)).ToArray();

            configsWithGroups.Add(new ProjectStructureConfigResponse(
                config.Id.Value.ToString(),
                config.Name.Value,
                config.ResourceGroupCount,
                config.ResourceCount,
                config.CrossConfigReferenceCount,
                resourceGroups));
        }

        var response = new ProjectStructureResponse(
            project.Id.Value.ToString(),
            project.Name.Value,
            project.LayoutPreset,
            project.EnvironmentDefinitions.Count,
            project.EnvironmentDefinitions.Select(e => new ProjectEnvironmentResponse(
                e.Name.Value,
                e.ShortName,
                e.Location)).ToArray(),
            configsWithGroups,
            configsWithGroups.Count);

        return JsonSerializer.Serialize(response, McpJsonDefaults.SerializerOptions);
    }

    /// <summary>
    /// Lists all resources across all resource groups in a project, with their IDs and types.
    /// </summary>
    [McpServerTool(Name = "list_project_resources")]
    [Description(
        "Lists all resources in a project across all configs and resource groups. " +
        "Returns resource ID, type, name, and parent resource group for each resource. " +
        "Use this when you need resource IDs for subsequent configuration calls.")]
    public static async Task<string> ListProjectResources(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("Optional: filter by resource type (e.g. 'ContainerApp', 'KeyVault').")] string? resourceTypeFilter = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(projectId, out var guid))
        {
            return McpJsonDefaults.Error("invalid_project_id", $"'{projectId}' is not a valid GUID.");
        }

        var pid = new ProjectId(guid);

        var configsResult = await mediator.Send(new ListProjectConfigsQuery(pid), cancellationToken);
        if (configsResult.IsError)
        {
            return McpJsonDefaults.Error("configs_error", string.Join("; ", configsResult.Errors.Select(e => e.Description)));
        }

        var allResources = new List<ProjectResourceResponse>();

        foreach (var config in configsResult.Value)
        {
            var rgResult = await mediator.Send(new ListResourceGroupsByConfigQuery(config.Id), cancellationToken);
            if (rgResult.IsError) continue;

            foreach (var rg in rgResult.Value)
            {
                foreach (var resource in rg.Resources)
                {
                    if (resourceTypeFilter is not null
                        && !string.Equals(resource.ResourceType.Value, resourceTypeFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    allResources.Add(new ProjectResourceResponse(
                        resource.Id.Value.ToString(),
                        resource.ResourceType.Value,
                        resource.Name.Value,
                        rg.Id.Value.ToString(),
                        rg.Name.Value,
                        config.Id.Value.ToString(),
                        config.Name.Value));
                }
            }
        }

        return JsonSerializer.Serialize(
            new ProjectResourcesResponse(projectId, allResources.Count, allResources),
            McpJsonDefaults.SerializerOptions);
    }

    private sealed record ProjectStructureResponse(
        string ProjectId,
        string ProjectName,
        string LayoutPreset,
        int EnvironmentCount,
        IReadOnlyList<ProjectEnvironmentResponse> Environments,
        IReadOnlyList<ProjectStructureConfigResponse> InfrastructureConfigs,
        int TotalConfigs);

    private sealed record ProjectEnvironmentResponse(
        string Name,
        string ShortName,
        string Location);

    private sealed record ProjectStructureConfigResponse(
        string InfraConfigId,
        string Name,
        int ResourceGroupCount,
        int ResourceCount,
        int CrossConfigReferenceCount,
        IReadOnlyList<ProjectResourceGroupResponse> ResourceGroups);

    private sealed record ProjectResourceGroupResponse(
        string ResourceGroupId,
        string Name,
        string Location,
        IReadOnlyList<ProjectResourceSummaryResponse> Resources,
        int ResourceCount);

    private sealed record ProjectResourceSummaryResponse(
        string ResourceId,
        string ResourceType,
        string Name);

    private sealed record ProjectResourcesResponse(
        string ProjectId,
        int TotalResources,
        IReadOnlyList<ProjectResourceResponse> Resources);

    private sealed record ProjectResourceResponse(
        string ResourceId,
        string ResourceType,
        string Name,
        string ResourceGroupId,
        string ResourceGroupName,
        string InfraConfigId,
        string InfraConfigName);
}
