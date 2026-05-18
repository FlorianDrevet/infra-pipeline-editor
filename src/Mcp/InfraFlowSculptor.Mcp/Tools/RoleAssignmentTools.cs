using System.ComponentModel;
using System.Text.Json;
using InfraFlowSculptor.Application.RoleAssignments.Commands.AddRoleAssignment;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Mcp.Common;
using MediatR;
using ModelContextProtocol.Server;

namespace InfraFlowSculptor.Mcp.Tools;

/// <summary>
/// Provides MCP tools for managing role assignments between resources.
/// </summary>
[McpServerToolType]
public sealed class RoleAssignmentTools
{
    private const string InvalidIdError = "invalid_id";

    private RoleAssignmentTools() { }

    /// <summary>
    /// Adds a role assignment from a source resource to a target resource.
    /// This defines RBAC permissions between Azure resources.
    /// </summary>
    [McpServerTool(Name = "add_role_assignment")]
    [Description(
        "Adds a role assignment from a source resource (the principal) to a target resource (the scope). " +
        "This defines RBAC access — for example, a ContainerApp (source) accessing a KeyVault (target) " +
        "with the 'Key Vault Secrets User' role. " +
        "managedIdentityType must be 'SystemAssigned' or 'UserAssigned'. " +
        "If 'UserAssigned', provide the userAssignedIdentityId.")]
    public static async Task<string> AddRoleAssignment(
        ISender mediator,
        [Description("The source resource ID (GUID) — the identity/principal requesting access.")] string sourceResourceId,
        [Description("The target resource ID (GUID) — the resource being accessed (scope).")] string targetResourceId,
        [Description("The role definition ID (GUID) — the Azure RBAC role to assign.")] string roleDefinitionId,
        [Description("Identity type: 'SystemAssigned' or 'UserAssigned'.")] string managedIdentityType,
        [Description("Optional: the User Assigned Identity resource ID (GUID). Required when managedIdentityType is 'UserAssigned'.")] string? userAssignedIdentityId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(sourceResourceId, out var sourceId))
        {
            return McpJsonDefaults.Error(InvalidIdError, "The sourceResourceId must be a valid GUID.");
        }

        if (!Guid.TryParse(targetResourceId, out var targetId))
        {
            return McpJsonDefaults.Error(InvalidIdError, "The targetResourceId must be a valid GUID.");
        }

        AzureResourceId? uaiId = null;
        if (!string.IsNullOrWhiteSpace(userAssignedIdentityId))
        {
            if (!Guid.TryParse(userAssignedIdentityId, out var uaiGuid))
            {
                return McpJsonDefaults.Error(InvalidIdError, "The userAssignedIdentityId must be a valid GUID.");
            }

            uaiId = AzureResourceId.Create(uaiGuid);
        }

        var command = new AddRoleAssignmentCommand(
            SourceResourceId: AzureResourceId.Create(sourceId),
            TargetResourceId: AzureResourceId.Create(targetId),
            ManagedIdentityType: managedIdentityType,
            RoleDefinitionId: roleDefinitionId,
            UserAssignedIdentityId: uaiId);

        var result = await mediator.Send(command, cancellationToken);

        return result.Match(
            assignment => JsonSerializer.Serialize(new
            {
                status = "success",
                roleAssignmentId = assignment.Id.ToString(),
                sourceResourceId,
                targetResourceId,
                roleDefinitionId,
                managedIdentityType,
            }, McpJsonDefaults.SerializerOptions),
            errors => McpJsonDefaults.Error("command_failed", string.Join("; ", errors.Select(e => e.Description))));
    }
}
