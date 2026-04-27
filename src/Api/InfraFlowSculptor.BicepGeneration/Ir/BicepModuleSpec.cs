namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Immutable specification of a Bicep module, agnostic of text formatting.
/// Built by <see cref="Builder.BicepModuleBuilder"/> and emitted to text by
/// <see cref="Emit.BicepEmitter"/>.
/// </summary>
public sealed record BicepModuleSpec
{
    /// <summary>Base module name before per-resource qualification (e.g. <c>keyVault</c>).</summary>
    public required string ModuleName { get; init; }

    /// <summary>Folder name under <c>modules/</c> (e.g. <c>KeyVault</c>).</summary>
    public required string ModuleFolderName { get; init; }

    /// <summary>Friendly resource type name used for naming template lookups (e.g. <c>KeyVault</c>).</summary>
    public required string ResourceTypeName { get; init; }

    /// <summary>Import statements at the top of the module (e.g. from <c>./types.bicep</c>).</summary>
    public IReadOnlyList<BicepImport> Imports { get; init; } = [];

    /// <summary>Parameter declarations in declaration order.</summary>
    public IReadOnlyList<BicepParam> Parameters { get; init; } = [];

    /// <summary>Variable declarations.</summary>
    public IReadOnlyList<BicepVar> Variables { get; init; } = [];

    /// <summary>Existing resource references used by the primary resource (e.g. parent lookups).</summary>
    public IReadOnlyList<BicepExistingResource> ExistingResources { get; init; } = [];

    /// <summary>The primary resource declaration for this module.</summary>
    public required BicepResourceDeclaration Resource { get; init; }

    /// <summary>Output declarations in declaration order.</summary>
    public IReadOnlyList<BicepOutput> Outputs { get; init; } = [];

    /// <summary>Type definitions emitted to <c>types.bicep</c>.</summary>
    public IReadOnlyList<BicepTypeDefinition> ExportedTypes { get; init; } = [];

    /// <summary>Companion modules deployed alongside this resource (e.g. blob/table services for Storage Account).</summary>
    public IReadOnlyList<BicepCompanionSpec> Companions { get; init; } = [];
}
