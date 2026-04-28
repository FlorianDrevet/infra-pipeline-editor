using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.Projects.Queries.GetProject;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Resources;

/// <summary>
/// Provides MCP resources for reading project data.
/// </summary>
[McpServerResourceType]
public sealed class ProjectResources
{
    private ProjectResources() { }

    /// <summary>
    /// Returns a structured summary of a project including its name, layout, environments, and resource types.
    /// </summary>
    [McpServerResource(
        UriTemplate = "ifs://projects/{projectId}/summary",
        Name = "Project Summary")]
    [Description("Returns a structured summary of a project including its name, layout, environments, and resource types.")]
    public static async Task<string> GetProjectSummary(
        ISender mediator,
        string projectId)
    {
        if (!Guid.TryParse(projectId, out var guid))
        {
            return JsonError("invalid_project_id", $"'{projectId}' is not a valid GUID.");
        }

        var query = new GetProjectQuery(new ProjectId(guid));
        var result = await mediator.Send(query);

        if (result.IsError)
        {
            return JsonError("project_not_found", string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var project = result.Value;
        return JsonSerializer.Serialize(new
        {
            projectId = project.Id.Value.ToString(),
            name = project.Name.Value,
            description = project.Description,
            layoutPreset = project.LayoutPreset,
            environmentCount = project.EnvironmentDefinitions.Count,
            environments = project.EnvironmentDefinitions.Select(e => new
            {
                name = e.Name.Value,
                shortName = e.ShortName,
                location = e.Location,
            }),
            repositoryCount = project.Repositories?.Count ?? 0,
            usedResourceTypes = project.UsedResourceTypes ?? [],
            agentPoolName = project.AgentPoolName,
        }, McpJsonDefaults.SerializerOptions);
    }

    private static string JsonError(string code, string message) =>
        McpJsonDefaults.Error(code, message);
}
