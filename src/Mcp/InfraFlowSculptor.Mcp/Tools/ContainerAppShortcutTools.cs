using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;
using InfraFlowSculptor.Application.ContainerApps.Queries.GetContainerApp;
using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides shortcut MCP tools for common Container App operations that otherwise
/// require multiple individual tool calls.
/// </summary>
[McpServerToolType]
public sealed class ContainerAppShortcutTools
{
    private const string AcrPullRoleDefinitionId = "7f951dda-4ed3-4680-a7ca-43fe172d538d";

    private ContainerAppShortcutTools() { }

    /// <summary>
    /// Links a Container App to an ACR with AcrPull role assignment via a User Assigned Identity.
    /// This is a shortcut that combines: updating the ContainerApp's ACR reference + creating the role assignment.
    /// </summary>
    [McpServerTool(Name = "link_container_app_to_acr")]
    [Description(
        "Links a Container App to a Container Registry with AcrPull access via a User Assigned Identity. " +
        "This shortcut performs two operations: " +
        "1) Updates the ContainerApp's containerRegistryId and acrPullIdentityId. " +
        "2) Creates an AcrPull role assignment (UAI → ACR). " +
        "Use this instead of manually calling add_role_assignment + updating the ContainerApp.")]
    public static async Task<string> LinkContainerAppToAcr(
        ISender mediator,
        [Description("The Container App resource ID (GUID).")] string containerAppId,
        [Description("The Container Registry resource ID (GUID).")] string containerRegistryId,
        [Description("The User Assigned Identity resource ID (GUID) that will pull from ACR.")] string userAssignedIdentityId,
        [Description("Optional: Docker image name (e.g. 'myapp-api'). If not set, the current value is preserved.")] string? dockerImageName = null,
        [Description("Optional: Dockerfile path relative to the repo root (e.g. 'src/Api/Dockerfile').")] string? dockerfilePath = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(containerAppId, out var caId))
        {
            return McpJsonDefaults.Error("invalid_id", "containerAppId must be a valid GUID.");
        }

        if (!Guid.TryParse(containerRegistryId, out var acrId))
        {
            return McpJsonDefaults.Error("invalid_id", "containerRegistryId must be a valid GUID.");
        }

        if (!Guid.TryParse(userAssignedIdentityId, out var uaiId))
        {
            return McpJsonDefaults.Error("invalid_id", "userAssignedIdentityId must be a valid GUID.");
        }

        // Step 1: Get current ContainerApp state
        var current = await mediator.Send(new GetContainerAppQuery(AzureResourceId.Create(caId)), cancellationToken);
        if (current.IsError)
        {
            return McpJsonDefaults.Error("container_app_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));
        }

        // Step 2: Update ContainerApp with ACR reference
        var updateCommand = new UpdateContainerAppCommand(
            Id: AzureResourceId.Create(caId),
            Name: current.Value.Name,
            Location: current.Value.Location,
            ContainerAppEnvironmentId: current.Value.ContainerAppEnvironmentId,
            ContainerRegistryId: acrId,
            AcrAuthMode: "ManagedIdentity",
            AcrPullIdentityId: uaiId,
            DockerImageName: dockerImageName ?? current.Value.DockerImageName,
            DockerImageValidated: current.Value.DockerImageValidated,
            DockerfilePath: dockerfilePath ?? current.Value.DockerfilePath,
            ApplicationName: current.Value.ApplicationName,
            EnvironmentSettings: current.Value.EnvironmentSettings);

        var updateResult = await mediator.Send(updateCommand, cancellationToken);
        if (updateResult.IsError)
        {
            return McpJsonDefaults.Error("update_failed", string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }

        // Step 3: Create AcrPull role assignment (UAI → ACR)
        var roleCommand = new AddRoleAssignmentCommand(
            SourceResourceId: AzureResourceId.Create(uaiId),
            TargetResourceId: AzureResourceId.Create(acrId),
            ManagedIdentityType: "UserAssigned",
            RoleDefinitionId: AcrPullRoleDefinitionId,
            UserAssignedIdentityId: AzureResourceId.Create(uaiId));

        var roleResult = await mediator.Send(roleCommand, cancellationToken);

        var roleStatus = roleResult.IsError
            ? $"Role assignment failed: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}"
            : "AcrPull role assigned successfully";

        return JsonSerializer.Serialize(new
        {
            status = "success",
            containerAppId,
            containerRegistryId,
            userAssignedIdentityId,
            acrAuthMode = "ManagedIdentity",
            dockerImageName = dockerImageName ?? current.Value.DockerImageName,
            dockerfilePath = dockerfilePath ?? current.Value.DockerfilePath,
            roleAssignmentStatus = roleStatus,
            nextSteps = new[]
            {
                "Configure per-environment settings with 'set_resource_environment_settings' (cpuCores, memoryGi, replicas, ingress).",
                "Add app settings with 'add_app_setting' or 'add_output_reference_app_setting'.",
                "Generate Bicep with 'generate_project_bicep'.",
            },
        }, McpJsonDefaults.SerializerOptions);
    }

    /// <summary>
    /// Configures Docker-specific settings on a Container App (image name, Dockerfile path, validated flag).
    /// </summary>
    [McpServerTool(Name = "set_container_app_docker_config")]
    [Description(
        "Sets the Docker image configuration on a Container App. " +
        "This updates the image name, Dockerfile path, and optionally marks the image as validated. " +
        "Use after creating a ContainerApp and linking it to an ACR.")]
    public static async Task<string> SetContainerAppDockerConfig(
        ISender mediator,
        [Description("The Container App resource ID (GUID).")] string containerAppId,
        [Description("Docker image name (without tag, e.g. 'vpd-api').")] string dockerImageName,
        [Description("Dockerfile path relative to the repo root (e.g. 'src/Api/Dockerfile').")] string? dockerfilePath = null,
        [Description("Whether the Docker image has been validated (builds successfully).")] bool dockerImageValidated = false,
        [Description("Optional application name (logical name for the container app workload).")] string? applicationName = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(containerAppId, out var caId))
        {
            return McpJsonDefaults.Error("invalid_id", "containerAppId must be a valid GUID.");
        }

        var current = await mediator.Send(new GetContainerAppQuery(AzureResourceId.Create(caId)), cancellationToken);
        if (current.IsError)
        {
            return McpJsonDefaults.Error("container_app_not_found", string.Join("; ", current.Errors.Select(e => e.Description)));
        }

        var command = new UpdateContainerAppCommand(
            Id: AzureResourceId.Create(caId),
            Name: current.Value.Name,
            Location: current.Value.Location,
            ContainerAppEnvironmentId: current.Value.ContainerAppEnvironmentId,
            ContainerRegistryId: current.Value.ContainerRegistryId,
            AcrAuthMode: current.Value.AcrAuthMode,
            AcrPullIdentityId: current.Value.AcrPullIdentityId,
            DockerImageName: dockerImageName,
            DockerImageValidated: dockerImageValidated,
            DockerfilePath: dockerfilePath ?? current.Value.DockerfilePath,
            ApplicationName: applicationName ?? current.Value.ApplicationName,
            EnvironmentSettings: current.Value.EnvironmentSettings);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            _ => JsonSerializer.Serialize(new
            {
                status = "success",
                containerAppId,
                dockerImageName,
                dockerfilePath = dockerfilePath ?? current.Value.DockerfilePath,
                dockerImageValidated,
                applicationName = applicationName ?? current.Value.ApplicationName,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("update_failed", string.Join("; ", errors.Select(e => e.Description))));
    }
}
