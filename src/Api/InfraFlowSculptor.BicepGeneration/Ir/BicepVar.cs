namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Bicep variable declaration (e.g. <c>var computedName = '${prefix}-${name}'</c>).
/// </summary>
public sealed record BicepVar(string Name, BicepExpression Expression);
