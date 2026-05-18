using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddCrossConfigReference;
using InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;
using InfraFlowSculptor.Application.ResourceGroups.Commands.CreateResourceGroup;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for creating infrastructure configurations, resource groups,
/// and managing cross-config references.
/// </summary>
[McpServerToolType]
public sealed class InfrastructureTools
{
    private const string InvalidIdError = "invalid_id";

    private InfrastructureTools() { }

    /// <summary>
    /// Creates a new infrastructure configuration within a project.
    /// </summary>
    [McpServerTool(Name = "create_infrastructure_config")]
    [Description(
        "Creates a new infrastructure configuration (deployment unit) within a project. " +
        "A project can have multiple infra configs, each containing its own resource groups and resources.")]
    public static async Task<string> CreateInfrastructureConfig(
        ISender mediator,
        [Description("The project ID (GUID).")] string projectId,
        [Description("Name of the infrastructure configuration (e.g. 'Core', 'Application').")] string name,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(projectId, out var id))
        {
            return McpJsonDefaults.Error(InvalidIdError, "The projectId must be a valid GUID.");
        }

        var command = new CreateInfrastructureConfigCommand(name, id);
        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            config => JsonSerializer.Serialize(new
            {
                status = "success",
                infrastructureConfigId = config.Id.ToString(),
                name = config.Name,
                projectId,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Creates a resource group within an infrastructure configuration.
    /// </summary>
    [McpServerTool(Name = "create_resource_group")]
    [Description(
        "Creates a resource group within an infrastructure configuration. " +
        "Resources are deployed into resource groups.")]
    public static async Task<string> CreateResourceGroup(
        ISender mediator,
        [Description("The infrastructure config ID (GUID).")] string infraConfigId,
        [Description("Resource group name (e.g. 'rg-myapp-core').")] string name,
        [Description("Azure region (e.g. 'FranceCentral', 'WestEurope').")] string location,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(infraConfigId, out var configId))
        {
            return McpJsonDefaults.Error(InvalidIdError, "The infraConfigId must be a valid GUID.");
        }

        if (!TryParseLocation(location, out var locationEnum))
        {
            return McpJsonDefaults.Error("invalid_location", $"'{location}' is not a supported Azure location.");
        }

        var command = new CreateResourceGroupCommand(
            InfraConfigId: new InfrastructureConfigId(configId),
            Name: new Name(name),
            Location: new Location(locationEnum));

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            rg => JsonSerializer.Serialize(new
            {
                status = "success",
                resourceGroupId = rg.Id.ToString(),
                name = rg.Name,
                location = rg.Location,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    /// <summary>
    /// Adds a cross-configuration resource reference to an infrastructure config.
    /// This allows resources in one config to reference resources in another config.
    /// </summary>
    [McpServerTool(Name = "add_cross_config_reference")]
    [Description(
        "Adds a cross-configuration reference to an infrastructure config. " +
        "This allows resources in this config to reference a resource from a different config " +
        "(e.g. a Log Analytics Workspace in 'Core' referenced by App Insights in 'Application').")]
    public static async Task<string> AddCrossConfigReference(
        ISender mediator,
        [Description("The infrastructure config ID (GUID) that will hold the reference.")] string infraConfigId,
        [Description("The target resource ID (GUID) from the other infrastructure config.")] string targetResourceId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(infraConfigId, out var configId))
        {
            return McpJsonDefaults.Error(InvalidIdError, "The infraConfigId must be a valid GUID.");
        }

        if (!Guid.TryParse(targetResourceId, out var targetId))
        {
            return McpJsonDefaults.Error(InvalidIdError, "The targetResourceId must be a valid GUID.");
        }

        var command = new AddCrossConfigReferenceCommand(configId, targetId);
        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            reference => JsonSerializer.Serialize(new
            {
                status = "success",
                crossConfigReferenceId = reference.Id.ToString(),
                infraConfigId,
                targetResourceId,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }

    private static bool TryParseLocation(string location, out Location.LocationEnum result)
    {
        return Enum.TryParse(location, ignoreCase: true, out result);
    }
}
