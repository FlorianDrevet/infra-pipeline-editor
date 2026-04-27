namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Bicep resource declaration (e.g. <c>resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = { ... }</c>).
/// The <see cref="Body"/> preserves property insertion order for deterministic emission.
/// </summary>
public sealed record BicepResourceDeclaration
{
    /// <summary>The Bicep symbol name for this resource (e.g. <c>kv</c>, <c>identity</c>).</summary>
    public required string Symbol { get; init; }

    /// <summary>The ARM resource type with API version (e.g. <c>Microsoft.KeyVault/vaults@2023-07-01</c>).</summary>
    public required string ArmTypeWithApiVersion { get; init; }

    /// <summary>
    /// Optional parent resource symbol. When set, emits <c>parent: symbolName</c> as the first body property.
    /// The parent must be declared as an existing resource in <see cref="BicepModuleSpec.ExistingResources"/>.
    /// </summary>
    public string? ParentSymbol { get; init; }

    /// <summary>
    /// Optional condition expression. When set, emits <c>= if (condition) {</c> instead of <c>= {</c>.
    /// </summary>
    public BicepExpression? Condition { get; init; }

    /// <summary>
    /// Optional scope symbol. When set, emits <c>scope: symbolName</c> as the first body property (after parent).
    /// Used for extension resources like <c>Microsoft.Insights/diagnosticSettings</c>.
    /// </summary>
    public string? Scope { get; init; }

    /// <summary>
    /// Optional for-loop. When set, emits <c>= [for iterator in collection: { ... }]</c> instead of <c>= { ... }</c>.
    /// </summary>
    public BicepForLoop? ForLoop { get; init; }

    /// <summary>
    /// Ordered list of top-level properties in the resource body (name, location, sku, identity, properties, tags, etc.).
    /// Property order is preserved for deterministic emission matching legacy templates.
    /// </summary>
    public IReadOnlyList<BicepPropertyAssignment> Body { get; init; } = [];
}
