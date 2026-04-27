namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Bicep parameter declaration (e.g. <c>@description('...') param name string = 'default'</c>).
/// </summary>
public sealed record BicepParam(
    string Name,
    BicepType Type,
    string? Description = null,
    bool IsSecure = false,
    BicepExpression? DefaultValue = null);
