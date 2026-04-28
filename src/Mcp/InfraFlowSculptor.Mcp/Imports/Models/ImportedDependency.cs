namespace InfraFlowSculptor.Mcp.Imports.Models;

/// <summary>A dependency relationship between two source resources.</summary>
public sealed record ImportedDependency(
    string FromResourceName,
    string ToResourceName,
    string DependencyType);
