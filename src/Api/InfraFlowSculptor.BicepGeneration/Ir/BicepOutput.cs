namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Bicep output declaration (e.g. <c>@description('...') output name string = expr</c>).
/// </summary>
public sealed record BicepOutput(
    string Name,
    BicepType Type,
    BicepExpression Expression,
    bool IsSecure = false,
    string? Description = null);
