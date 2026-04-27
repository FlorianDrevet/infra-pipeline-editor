namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Represents a Bicep type used in parameter or output declarations.
/// </summary>
public abstract record BicepType
{
    /// <summary>Bicep <c>string</c> type.</summary>
    public static readonly BicepType String = new BicepPrimitiveType("string");

    /// <summary>Bicep <c>int</c> type.</summary>
    public static readonly BicepType Int = new BicepPrimitiveType("int");

    /// <summary>Bicep <c>bool</c> type.</summary>
    public static readonly BicepType Bool = new BicepPrimitiveType("bool");

    /// <summary>Bicep <c>object</c> type.</summary>
    public static readonly BicepType Object = new BicepPrimitiveType("object");

    /// <summary>Bicep <c>array</c> type.</summary>
    public static readonly BicepType Array = new BicepPrimitiveType("array");

    /// <summary>Creates a custom named Bicep type (e.g. an <c>@export()</c>-ed type from <c>types.bicep</c>).</summary>
    public static BicepType Custom(string name) => new BicepCustomType(name);
}

/// <summary>Bicep built-in primitive type (<c>string</c>, <c>int</c>, <c>bool</c>, <c>object</c>, <c>array</c>).</summary>
public sealed record BicepPrimitiveType(string Name) : BicepType;

/// <summary>Custom named Bicep type (e.g. <c>SkuName</c>, <c>ManagedIdentityType</c>).</summary>
public sealed record BicepCustomType(string Name) : BicepType;
