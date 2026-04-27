namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Reference to a role definition by name with its service category for constants lookup.
/// </summary>
internal sealed record RoleRef(string RoleDefinitionName, string ServiceCategory);
