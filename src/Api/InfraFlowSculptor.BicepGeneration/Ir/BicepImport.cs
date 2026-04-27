namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Bicep import statement (e.g. <c>import { SkuName, PublicNetworkAccess } from './types.bicep'</c>).
/// </summary>
public sealed record BicepImport(string Path, IReadOnlyList<string>? Symbols = null);
