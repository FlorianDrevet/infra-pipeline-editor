using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectDefaultNamingTemplate;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;
using InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceNamingTemplate;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for managing project-level naming templates and resource abbreviations.
/// These control how Azure resource names are generated in Bicep output.
/// </summary>
[McpServerToolType]
public sealed class NamingTools
{
    private NamingTools() { }

    /// <summary>
    /// Sets the default naming template for a project.
    /// Placeholders: {projectName}, {resourceName}, {resourceAbbr}, {envPrefix}, {envSuffix}, {location}.
    /// Example: "{projectName}-{resourceAbbr}-{envSuffix}"
    /// </summary>
    [McpServerTool(Name = "set_project_naming_template")]
    [Description(
        "Sets the default naming template for a project. " +
        "Placeholders: {projectName}, {resourceName}, {resourceAbbr}, {envPrefix}, {envSuffix}, {location}. " +
        "Example: '{projectName}-{resourceAbbr}-{envSuffix}'. " +
        "Resource names in the project should be SHORT identifiers (e.g. 'api', 'frontend') because the naming template adds the project name and abbreviation automatically.")]
    public static async Task<string> SetProjectNamingTemplate(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("The naming template string with placeholders. Pass null or empty to clear.")] string? template)
    {
        if (!Guid.TryParse(projectId, out var id))
        {
            return McpJsonDefaults.Error("invalid_project_id", "The projectId must be a valid GUID.");
        }

        var command = new SetProjectDefaultNamingTemplateCommand(new ProjectId(id), template);
        var result = await mediator.Send(command);

        return result.Match(
            _ => JsonSerializer.Serialize(new { status = "success", message = "Default naming template updated." }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Sets a per-resource-type naming template override for a project.
    /// </summary>
    [McpServerTool(Name = "set_project_resource_naming_template")]
    [Description(
        "Sets a naming template override for a specific resource type in the project. " +
        "Use this when a resource type needs a different naming pattern than the project default. " +
        "Placeholders: {projectName}, {resourceName}, {resourceAbbr}, {envPrefix}, {envSuffix}, {location}.")]
    public static async Task<string> SetProjectResourceNamingTemplate(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("The resource type (e.g. 'KeyVault', 'ContainerApp', 'SqlServer').")] string resourceType,
        [Description("The naming template string with placeholders.")] string template)
    {
        if (!Guid.TryParse(projectId, out var id))
        {
            return McpJsonDefaults.Error("invalid_project_id", "The projectId must be a valid GUID.");
        }

        var command = new SetProjectResourceNamingTemplateCommand(new ProjectId(id), resourceType, template);
        var result = await mediator.Send(command);

        return result.Match(
            tpl => JsonSerializer.Serialize(new { status = "success", resourceType, template }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Removes a per-resource-type naming template override from a project.
    /// </summary>
    [McpServerTool(Name = "remove_project_resource_naming_template")]
    [Description("Removes a per-resource-type naming template override, reverting to the project default.")]
    public static async Task<string> RemoveProjectResourceNamingTemplate(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("The resource type to remove the override for.")] string resourceType)
    {
        if (!Guid.TryParse(projectId, out var id))
        {
            return McpJsonDefaults.Error("invalid_project_id", "The projectId must be a valid GUID.");
        }

        var command = new RemoveProjectResourceNamingTemplateCommand(new ProjectId(id), resourceType);
        var result = await mediator.Send(command);

        return result.Match(
            _ => JsonSerializer.Serialize(new { status = "success", message = $"Naming template override for '{resourceType}' removed." }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Sets or updates a resource abbreviation for the {resourceAbbr} placeholder in naming templates.
    /// </summary>
    [McpServerTool(Name = "set_project_resource_abbreviation")]
    [Description(
        "Sets or updates the abbreviation used in the {resourceAbbr} placeholder for a specific resource type. " +
        "Must be lowercase alphanumeric, max 10 characters. " +
        "Example: 'kv' for KeyVault, 'acr' for ContainerRegistry, 'sql' for SqlServer.")]
    public static async Task<string> SetProjectResourceAbbreviation(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("The resource type (e.g. 'KeyVault', 'ContainerApp').")] string resourceType,
        [Description("The abbreviation string (lowercase alphanumeric, max 10 chars).")] string abbreviation)
    {
        if (!Guid.TryParse(projectId, out var id))
        {
            return McpJsonDefaults.Error("invalid_project_id", "The projectId must be a valid GUID.");
        }

        var command = new SetProjectResourceAbbreviationCommand(new ProjectId(id), resourceType, abbreviation);
        var result = await mediator.Send(command);

        return result.Match(
            _ => JsonSerializer.Serialize(new { status = "success", resourceType, abbreviation }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Removes a resource abbreviation override from a project (reverts to the system default).
    /// </summary>
    [McpServerTool(Name = "remove_project_resource_abbreviation")]
    [Description("Removes a resource abbreviation override for a specific resource type, reverting to the system default.")]
    public static async Task<string> RemoveProjectResourceAbbreviation(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("The resource type to remove the abbreviation override for.")] string resourceType)
    {
        if (!Guid.TryParse(projectId, out var id))
        {
            return McpJsonDefaults.Error("invalid_project_id", "The projectId must be a valid GUID.");
        }

        var command = new RemoveProjectResourceAbbreviationCommand(new ProjectId(id), resourceType);
        var result = await mediator.Send(command);

        return result.Match(
            _ => JsonSerializer.Serialize(new { status = "success", message = $"Abbreviation override for '{resourceType}' removed." }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }
}
