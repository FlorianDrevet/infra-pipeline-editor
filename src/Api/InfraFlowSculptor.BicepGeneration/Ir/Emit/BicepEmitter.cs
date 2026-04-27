using System.Text;

namespace InfraFlowSculptor.BicepGeneration.Ir.Emit;

/// <summary>
/// Transforms a <see cref="BicepModuleSpec"/> into Bicep text. This is the single point
/// that decides formatting (indentation, ordering, spacing). The emitter produces output
/// that must match the legacy <c>const string</c> templates byte-for-byte for parity tests.
/// </summary>
public sealed class BicepEmitter
{
    /// <summary>
    /// Emits the primary module content (<c>*.module.bicep</c>) from the given spec.
    /// </summary>
    public string EmitModule(BicepModuleSpec spec)
    {
        var sb = new StringBuilder();

        EmitImports(sb, spec.Imports);
        EmitParameters(sb, spec.Parameters);
        EmitVariables(sb, spec.Variables);
        EmitExistingResources(sb, spec.ExistingResources);
        EmitResource(sb, spec.Resource);
        EmitOutputs(sb, spec.Outputs);

        return sb.ToString().TrimEnd('\n', '\r') + "\n";
    }

    /// <summary>
    /// Emits the types content (<c>types.bicep</c>) from the given spec.
    /// Returns empty string when no types are defined.
    /// </summary>
    public string EmitTypes(BicepModuleSpec spec)
    {
        if (spec.ExportedTypes.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        for (var i = 0; i < spec.ExportedTypes.Count; i++)
        {
            var typeDef = spec.ExportedTypes[i];
            if (i > 0)
                sb.AppendLine();

            if (typeDef.IsExported)
                sb.AppendLine("@export()");

            if (typeDef.Description is not null)
                sb.Append("@description('").Append(EscapeBicepString(typeDef.Description)).AppendLine("')");

            sb.Append("type ").Append(typeDef.Name).Append(" = ").AppendLine(EmitExpression(typeDef.Body, indent: 0));
        }

        return sb.ToString().TrimEnd('\n', '\r') + "\n";
    }

    private void EmitImports(StringBuilder sb, IReadOnlyList<BicepImport> imports)
    {
        if (imports.Count == 0)
            return;

        foreach (var import in imports)
        {
            if (import.Symbols is { Count: > 0 })
            {
                sb.Append("import { ").Append(string.Join(", ", import.Symbols)).Append(" } from '").Append(import.Path).AppendLine("'");
            }
            else
            {
                sb.Append("import '").Append(import.Path).AppendLine("'");
            }
        }

        sb.AppendLine();
    }

    private void EmitParameters(StringBuilder sb, IReadOnlyList<BicepParam> parameters)
    {
        for (var i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];

            if (param.IsSecure)
                sb.AppendLine("@secure()");

            if (param.Description is not null)
                sb.Append("@description('").Append(EscapeBicepString(param.Description)).AppendLine("')");

            sb.Append("param ").Append(param.Name).Append(' ').Append(EmitType(param.Type));

            if (param.DefaultValue is not null)
            {
                sb.Append(" = ").Append(EmitExpression(param.DefaultValue, indent: 0));
            }

            sb.AppendLine();

            // Blank line after each param
            sb.AppendLine();
        }
    }

    private static void EmitVariables(StringBuilder sb, IReadOnlyList<BicepVar> variables)
    {
        if (variables.Count == 0)
            return;

        foreach (var variable in variables)
        {
            sb.Append("var ").Append(variable.Name).Append(" = ").AppendLine(EmitExpression(variable.Expression, indent: 0));
        }

        sb.AppendLine();
    }

    private static void EmitExistingResources(StringBuilder sb, IReadOnlyList<BicepExistingResource> existingResources)
    {
        if (existingResources.Count == 0)
            return;

        foreach (var existing in existingResources)
        {
            sb.Append("resource ").Append(existing.Symbol).Append(" '").Append(existing.ArmTypeWithApiVersion).AppendLine("' existing = {");
            sb.Append("  name: ").AppendLine(existing.NameExpression);
            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    private static void EmitResource(StringBuilder sb, BicepResourceDeclaration resource)
    {
        sb.Append("resource ").Append(resource.Symbol).Append(" '").Append(resource.ArmTypeWithApiVersion).AppendLine("' = {");

        if (resource.ParentSymbol is not null)
        {
            sb.Append("  parent: ").AppendLine(resource.ParentSymbol);
        }

        foreach (var prop in resource.Body)
        {
            EmitPropertyAssignment(sb, prop, indent: 2);
        }

        sb.AppendLine("}");
    }

    private static void EmitOutputs(StringBuilder sb, IReadOnlyList<BicepOutput> outputs)
    {
        if (outputs.Count == 0)
            return;

        for (var i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];
            var hasDecorator = output.IsSecure || output.Description is not null;

            // Blank line before output (separator from resource or previous output)
            sb.AppendLine();

            if (output.IsSecure)
                sb.AppendLine("@secure()");

            if (output.Description is not null)
                sb.Append("@description('").Append(EscapeBicepString(output.Description)).AppendLine("')");

            sb.Append("output ").Append(output.Name).Append(' ').Append(EmitType(output.Type))
                .Append(" = ").Append(EmitExpression(output.Expression, indent: 0));
            sb.AppendLine();
        }
    }

    private static void EmitPropertyAssignment(StringBuilder sb, BicepPropertyAssignment prop, int indent)
    {
        var prefix = new string(' ', indent);
        sb.Append(prefix).Append(prop.Key).Append(": ");

        if (prop.Value is BicepObjectExpression obj && obj.Properties.Count > 0)
        {
            sb.AppendLine("{");
            foreach (var nested in obj.Properties)
            {
                EmitPropertyAssignment(sb, nested, indent + 2);
            }
            sb.Append(prefix).AppendLine("}");
        }
        else if (prop.Value is BicepArrayExpression arr && arr.Items.Count > 0)
        {
            EmitArrayMultiline(sb, arr, indent);
        }
        else
        {
            sb.AppendLine(EmitExpression(prop.Value, indent));
        }
    }

    private static void EmitArrayMultiline(StringBuilder sb, BicepArrayExpression arr, int indent)
    {
        var prefix = new string(' ', indent);
        sb.AppendLine("[");
        foreach (var item in arr.Items)
        {
            sb.Append(prefix).Append("  ");
            if (item is BicepObjectExpression obj && obj.Properties.Count > 0)
            {
                sb.AppendLine("{");
                foreach (var nested in obj.Properties)
                {
                    EmitPropertyAssignment(sb, nested, indent + 4);
                }
                sb.Append(prefix).Append("  ").AppendLine("}");
            }
            else
            {
                sb.AppendLine(EmitExpression(item, indent + 2));
            }
        }
        sb.Append(prefix).AppendLine("]");
    }

    internal static string EmitExpression(BicepExpression expr, int indent) => expr switch
    {
        BicepStringLiteral s => $"'{EscapeBicepString(s.Value)}'",
        BicepIntLiteral i => i.Value.ToString(),
        BicepBoolLiteral b => b.Value ? "true" : "false",
        BicepReference r => r.Symbol,
        BicepRawExpression raw => raw.RawBicep,
        BicepInterpolation interp => EmitInterpolation(interp),
        BicepFunctionCall fn => EmitFunctionCall(fn, indent),
        BicepConditionalExpression cond => $"{EmitExpression(cond.Condition, indent)} ? {EmitExpression(cond.Consequent, indent)} : {EmitExpression(cond.Alternate, indent)}",
        BicepObjectExpression obj when obj.Properties.Count == 0 => "{}",
        BicepObjectExpression obj => EmitInlineObject(obj, indent),
        BicepArrayExpression arr when arr.Items.Count == 0 => "[]",
        BicepArrayExpression arr => EmitInlineArray(arr, indent),
        _ => throw new NotSupportedException($"Unsupported expression type: {expr.GetType().Name}"),
    };

    private static string EmitInterpolation(BicepInterpolation interp)
    {
        var sb = new StringBuilder();
        sb.Append('\'');
        foreach (var part in interp.Parts)
        {
            if (part is BicepStringLiteral literal)
            {
                sb.Append(EscapeBicepString(literal.Value));
            }
            else
            {
                sb.Append("${").Append(EmitExpression(part, indent: 0)).Append('}');
            }
        }
        sb.Append('\'');
        return sb.ToString();
    }

    private static string EmitFunctionCall(BicepFunctionCall fn, int indent)
    {
        var args = string.Join(", ", fn.Arguments.Select(a => EmitExpression(a, indent)));
        return $"{fn.Name}({args})";
    }

    private static string EmitInlineObject(BicepObjectExpression obj, int indent)
    {
        var props = string.Join(", ", obj.Properties.Select(p =>
            $"{p.Key}: {EmitExpression(p.Value, indent)}"));
        return $"{{ {props} }}";
    }

    private static string EmitInlineArray(BicepArrayExpression arr, int indent)
    {
        var items = string.Join(", ", arr.Items.Select(i => EmitExpression(i, indent)));
        return $"[{items}]";
    }

    private static string EmitType(BicepType type) => type switch
    {
        BicepPrimitiveType p => p.Name,
        BicepCustomType c => c.Name,
        _ => throw new NotSupportedException($"Unsupported Bicep type: {type.GetType().Name}"),
    };

    private static string EscapeBicepString(string value)
        => value.Replace("\\", "\\\\").Replace("'", "\\'");
}
