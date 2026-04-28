using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBicep;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for generating Bicep infrastructure-as-code from existing projects.
/// </summary>
[McpServerToolType]
public sealed class BicepGenerationTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Generates Bicep infrastructure-as-code files for an existing project.
    /// Returns a summary of generated files organized by common shared files and per-configuration files.
    /// </summary>
    [McpServerTool(Name = "generate_project_bicep")]
    [Description("Generates Bicep infrastructure-as-code files for an existing project. Returns a summary of generated files.")]
    public static async Task<string> GenerateProjectBicep(
        ISender mediator,
        [Description("The project ID (GUID format).")] string projectId)
    {
        if (!Guid.TryParse(projectId, out var guid))
        {
            return JsonError("invalid_project_id", $"'{projectId}' is not a valid GUID.");
        }

        var command = new GenerateProjectBicepCommand(new ProjectId(guid));
        var result = await mediator.Send(command);

        if (result.IsError)
        {
            return JsonError("generation_failed", string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var value = result.Value;
        var commonFiles = value.CommonFileUris.Keys.ToList();
        var configFiles = value.ConfigFileUris.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Keys.ToList() as IReadOnlyList<string>);

        return JsonSerializer.Serialize(new
        {
            status = "generated",
            projectId,
            commonFiles,
            configFiles,
            totalFileCount = commonFiles.Count + configFiles.Values.Sum(v => v.Count),
        }, JsonOptions);
    }

    private static string JsonError(string code, string message) =>
        JsonSerializer.Serialize(new { error = code, message }, JsonOptions);
}
