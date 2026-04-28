namespace InfraFlowSculptor.Mcp.Common;

/// <summary>
/// Metadata record for a supported Azure resource type.
/// </summary>
/// <param name="Id">Resource type identifier matching <c>AzureResourceTypes</c> constants.</param>
/// <param name="ArmType">ARM provider resource type string.</param>
/// <param name="Label">Human-readable label.</param>
/// <param name="Category">Functional category (e.g. Compute, Data, Security).</param>
internal sealed record ResourceTypeInfo(
    string Id,
    string ArmType,
    string Label,
    string Category);
