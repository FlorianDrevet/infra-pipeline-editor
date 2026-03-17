namespace BicepGenerator.Domain;

public sealed record RoleAssignmentDefinition
{
    public string SourceResourceName { get; init; } = string.Empty;
    public string SourceResourceType { get; init; } = string.Empty;
    public string SourceResourceGroupName { get; init; } = string.Empty;
    public string TargetResourceName { get; init; } = string.Empty;
    public string TargetResourceType { get; init; } = string.Empty;
    public string TargetResourceGroupName { get; init; } = string.Empty;
    public string ManagedIdentityType { get; init; } = "SystemAssigned";
    public string RoleDefinitionId { get; init; } = string.Empty;
}
