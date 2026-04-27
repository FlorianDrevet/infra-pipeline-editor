using System.Collections.Immutable;

namespace InfraFlowSculptor.BicepGeneration.Ir;

/// <summary>
/// Base type for Bicep expressions in the IR. Each subclass represents a different
/// syntactic form in Bicep (literal, reference, interpolation, function call, etc.).
/// </summary>
public abstract record BicepExpression;

/// <summary>Bicep string literal (e.g. <c>'hello'</c>).</summary>
public sealed record BicepStringLiteral(string Value) : BicepExpression;

/// <summary>Bicep integer literal (e.g. <c>42</c>).</summary>
public sealed record BicepIntLiteral(int Value) : BicepExpression;

/// <summary>Bicep boolean literal (<c>true</c> or <c>false</c>).</summary>
public sealed record BicepBoolLiteral(bool Value) : BicepExpression;

/// <summary>Bicep symbol reference (e.g. <c>name</c>, <c>kv.id</c>, <c>kv.properties.vaultUri</c>).</summary>
public sealed record BicepReference(string Symbol) : BicepExpression;

/// <summary>
/// Bicep string interpolation (e.g. <c>'prefix-${name}-suffix'</c>).
/// Parts alternate between <see cref="BicepStringLiteral"/> (text) and expression nodes.
/// </summary>
public sealed record BicepInterpolation(IReadOnlyList<BicepExpression> Parts) : BicepExpression;

/// <summary>Bicep function call (e.g. <c>tenant()</c>, <c>concat(a, b)</c>).</summary>
public sealed record BicepFunctionCall(string Name, IReadOnlyList<BicepExpression> Arguments) : BicepExpression;

/// <summary>
/// Bicep object expression with ordered properties (e.g. <c>{ name: name, location: location }</c>).
/// Property order is preserved for deterministic emission.
/// </summary>
public sealed record BicepObjectExpression(IReadOnlyList<BicepPropertyAssignment> Properties) : BicepExpression
{
    /// <summary>Creates an empty object expression (<c>{}</c>).</summary>
    public static readonly BicepObjectExpression Empty = new([]);
}

/// <summary>Bicep array expression (e.g. <c>[item1, item2]</c>).</summary>
public sealed record BicepArrayExpression(IReadOnlyList<BicepExpression> Items) : BicepExpression;

/// <summary>Bicep ternary conditional expression (e.g. <c>cond ? a : b</c>).</summary>
public sealed record BicepConditionalExpression(
    BicepExpression Condition,
    BicepExpression Consequent,
    BicepExpression Alternate) : BicepExpression;

/// <summary>
/// Raw Bicep text injected verbatim by the emitter. Escape hatch for expressions
/// that cannot be modeled by the typed IR hierarchy.
/// </summary>
public sealed record BicepRawExpression(string RawBicep) : BicepExpression;

/// <summary>A single key-value property assignment inside a <see cref="BicepObjectExpression"/>.</summary>
public sealed record BicepPropertyAssignment(string Key, BicepExpression Value);
