namespace InfraFlowSculptor.Application.Imports.Common.Analysis;

/// <summary>
/// Represents a dependency between two parsed resources.
/// </summary>
/// <param name="FromResourceName">The source resource that depends on another resource.</param>
/// <param name="ToResourceName">The target resource name.</param>
/// <param name="DependencyType">The dependency type.</param>
public sealed record ImportedDependencyAnalysisResult(
    string FromResourceName,
    string ToResourceName,
    string DependencyType);
