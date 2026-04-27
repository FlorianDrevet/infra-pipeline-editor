namespace InfraFlowSculptor.BicepGeneration.Ir.Transformations;

/// <summary>
/// Pure transformations on <see cref="BicepModuleSpec"/> for output injection.
/// Replaces the legacy regex-based text manipulation operations.
/// </summary>
internal static class OutputTransformer
{
    /// <summary>
    /// Adds output declarations to the spec for outputs referenced by app settings on other resources.
    /// Skips outputs that already exist.
    /// </summary>
    internal static BicepModuleSpec WithOutputs(
        this BicepModuleSpec spec,
        IReadOnlyCollection<(string OutputName, string BicepExpression, bool IsSecure)> outputs)
    {
        if (outputs.Count == 0)
            return spec;

        var existingOutputNames = spec.Outputs.Select(o => o.Name).ToHashSet(StringComparer.Ordinal);
        var declaredSymbol = spec.Resource.Symbol;
        var newOutputs = spec.Outputs.ToList();

        foreach (var (outputName, bicepExpression, isSecure) in outputs)
        {
            if (existingOutputNames.Contains(outputName))
                continue;

            var rootSymbol = ExtractRootSymbol(bicepExpression);
            if (rootSymbol is not null && !string.Equals(rootSymbol, declaredSymbol, StringComparison.Ordinal))
                continue;

            newOutputs.Add(new BicepOutput(
                outputName,
                BicepType.String,
                new BicepRawExpression(bicepExpression),
                isSecure));
        }

        return spec with { Outputs = newOutputs };
    }

    private static string? ExtractRootSymbol(string bicepExpression)
    {
        var expr = bicepExpression.Trim();
        if (expr.Length == 0)
            return null;

        if (char.IsLetter(expr[0]) || expr[0] == '_')
        {
            var dotIndex = expr.IndexOf('.');
            return dotIndex > 0 ? expr[..dotIndex] : null;
        }

        var interpIdx = expr.IndexOf("${", StringComparison.Ordinal);
        if (interpIdx >= 0)
        {
            var start = interpIdx + 2;
            var rest = expr.AsSpan(start);
            var dotPos = rest.IndexOf('.');
            if (dotPos > 0)
                return rest[..dotPos].ToString();
        }

        return null;
    }
}
