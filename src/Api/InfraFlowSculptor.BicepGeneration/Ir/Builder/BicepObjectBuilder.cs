namespace InfraFlowSculptor.BicepGeneration.Ir.Builder;

/// <summary>
/// Helper for building <see cref="BicepObjectExpression"/> instances with ordered properties.
/// </summary>
public sealed class BicepObjectBuilder
{
    private readonly List<BicepPropertyAssignment> _properties = [];

    /// <summary>Adds a property with the given key and value expression.</summary>
    public BicepObjectBuilder Property(string key, BicepExpression value)
    {
        _properties.Add(new BicepPropertyAssignment(key, value));
        return this;
    }

    /// <summary>Adds a property with a string literal value.</summary>
    public BicepObjectBuilder Property(string key, string literalValue)
    {
        _properties.Add(new BicepPropertyAssignment(key, new BicepStringLiteral(literalValue)));
        return this;
    }

    /// <summary>Adds a property with a boolean literal value.</summary>
    public BicepObjectBuilder Property(string key, bool literalValue)
    {
        _properties.Add(new BicepPropertyAssignment(key, new BicepBoolLiteral(literalValue)));
        return this;
    }

    /// <summary>Adds a property with an integer literal value.</summary>
    public BicepObjectBuilder Property(string key, int literalValue)
    {
        _properties.Add(new BicepPropertyAssignment(key, new BicepIntLiteral(literalValue)));
        return this;
    }

    /// <summary>Adds a property with a nested object value built by the given action.</summary>
    public BicepObjectBuilder Property(string key, Action<BicepObjectBuilder> nestedBuilder)
    {
        var nested = new BicepObjectBuilder();
        nestedBuilder(nested);
        _properties.Add(new BicepPropertyAssignment(key, nested.Build()));
        return this;
    }

    /// <summary>Builds the immutable <see cref="BicepObjectExpression"/>.</summary>
    public BicepObjectExpression Build() => new(_properties.ToList());
}
