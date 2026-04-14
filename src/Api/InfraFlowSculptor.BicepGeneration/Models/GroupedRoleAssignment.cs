namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Groups role assignments by (source, target, identity type) for module declaration batching.
/// </summary>
internal sealed class GroupedRoleAssignment
{
    public string SourceResourceName { get; init; } = string.Empty;
    public string SourceResourceType { get; init; } = string.Empty;
    public string TargetResourceName { get; init; } = string.Empty;
    public string TargetResourceType { get; init; } = string.Empty;
    public string TargetResourceTypeName { get; init; } = string.Empty;
    public string TargetResourceGroupName { get; init; } = string.Empty;
    public string TargetResourceAbbreviation { get; init; } = string.Empty;
    public string ServiceCategory { get; init; } = string.Empty;
    public string ManagedIdentityType { get; init; } = string.Empty;
    public string? UserAssignedIdentityName { get; init; }
    public bool IsTargetCrossConfig { get; init; }
    public List<RoleRef> Roles { get; init; } = [];
}
