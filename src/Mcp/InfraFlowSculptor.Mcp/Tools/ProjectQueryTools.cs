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

        var configsWithGroups = new List<object>();

        foreach (var config in configsResult.Value)
        {
            var rgResult = await mediator.Send(new ListResourceGroupsByConfigQuery(config.Id), cancellationToken);

            var resourceGroups = rgResult.IsError
                ? []
                : rgResult.Value.Select(rg => new
                {
                    resourceGroupId = rg.Id.Value.ToString(),
                    name = rg.Name.Value,
                    location = rg.Location.Value.ToString(),
                    resources = rg.Resources.Select(r => new
                    {
                        resourceId = r.Id.Value.ToString(),
                        resourceType = r.ResourceType.Value,
                        name = r.Name.Value,
                    }).ToArray(),
                    resourceCount = rg.Resources.Length,
                }).ToArray();

            configsWithGroups.Add(new
            {
                infraConfigId = config.Id.Value.ToString(),
                name = config.Name.Value,
                resourceGroupCount = config.ResourceGroupCount,
                resourceCount = config.ResourceCount,
                crossConfigReferenceCount = config.CrossConfigReferenceCount,
                resourceGroups,
            });
        }

        return JsonSerializer.Serialize(new
        {
            projectId = project.Id.Value.ToString(),
            projectName = project.Name.Value,
            layoutPreset = project.LayoutPreset,
            environmentCount = project.EnvironmentDefinitions.Count,
            environments = project.EnvironmentDefinitions.Select(e => new
            {
                name = e.Name.Value,
                shortName = e.ShortName,
                location = e.Location,
            }),
            infrastructureConfigs = configsWithGroups,
            totalConfigs = configsWithGroups.Count,
        }, McpJsonDefaults.SerializerOptions);
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

        var allResources = new List<object>();

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

                    allResources.Add(new
                    {
                        resourceId = resource.Id.Value.ToString(),
                        resourceType = resource.ResourceType.Value,
                        name = resource.Name.Value,
                        resourceGroupId = rg.Id.Value.ToString(),
                        resourceGroupName = rg.Name.Value,
                        infraConfigId = config.Id.Value.ToString(),
                        infraConfigName = config.Name.Value,
                    });
                }
            }
        }

        return JsonSerializer.Serialize(new
        {
            projectId,
            totalResources = allResources.Count,
            resources = allResources,
        }, McpJsonDefaults.SerializerOptions);
    }
}
