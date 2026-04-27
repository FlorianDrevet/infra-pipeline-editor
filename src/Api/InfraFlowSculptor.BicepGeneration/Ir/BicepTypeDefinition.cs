namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Bicep type definition for <c>types.bicep</c>
/// (e.g. <c>@export() @description('...') type SkuName = 'Basic' | 'Standard' | 'Premium'</c>).
/// </summary>
public sealed record BicepTypeDefinition(
    string Name,
    BicepExpression Body,
    bool IsExported = false,
    string? Description = null);
