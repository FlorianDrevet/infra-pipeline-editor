namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Represents a role assignment between two Azure resources for Bicep generation.
/// The source resource's managed identity is granted a role on the target resource.
/// </summary>
public sealed class RoleAssignmentDefinition
{
    /// <summary>Name of the source resource whose identity holds the role.</summary>
    public string SourceResourceName { get; init; } = string.Empty;

    /// <summary>Azure resource type of the source (e.g. "Microsoft.Web/sites").</summary>
    public string SourceResourceType { get; init; } = string.Empty;

    /// <summary>Resource group name of the source resource.</summary>
    public string SourceResourceGroupName { get; init; } = string.Empty;

    /// <summary>Name of the target resource on which the role is granted.</summary>
    public string TargetResourceName { get; init; } = string.Empty;

    /// <summary>Azure resource type of the target (e.g. "Microsoft.KeyVault/vaults").</summary>
    public string TargetResourceType { get; init; } = string.Empty;

    /// <summary>Resource group name of the target resource.</summary>
    public string TargetResourceGroupName { get; init; } = string.Empty;

    /// <summary>"SystemAssigned" or "UserAssigned".</summary>
    public string ManagedIdentityType { get; init; } = string.Empty;

    /// <summary>Azure built-in role definition GUID.</summary>
    public string RoleDefinitionId { get; init; } = string.Empty;

    /// <summary>Human-readable role name (e.g. "Key Vault Secrets User").</summary>
    public string RoleDefinitionName { get; init; } = string.Empty;

    /// <summary>Description of the role definition.</summary>
    public string RoleDefinitionDescription { get; init; } = string.Empty;

    /// <summary>Bicep service category key for the constants.bicep grouping (e.g. "keyvault").</summary>
    public string ServiceCategory { get; init; } = string.Empty;

    /// <summary>Name of the User-Assigned Identity resource (when ManagedIdentityType is UserAssigned).</summary>
    public string? UserAssignedIdentityName { get; init; }

    /// <summary>Resource group of the User-Assigned Identity (when ManagedIdentityType is UserAssigned).</summary>
    public string? UserAssignedIdentityResourceGroupName { get; init; }

    /// <summary>Resource abbreviation of the target resource (for naming expression).</summary>
    public string TargetResourceAbbreviation { get; init; } = string.Empty;

    /// <summary>Simple type name of the source resource (e.g. "ContainerApp").</summary>
    public string SourceResourceTypeName { get; init; } = string.Empty;

    /// <summary>Simple type name of the target resource (e.g. "KeyVault").</summary>
    public string TargetResourceTypeName { get; init; } = string.Empty;

    /// <summary>Whether the target resource belongs to a different (cross-config) configuration.</summary>
    public bool IsTargetCrossConfig { get; init; }
}
