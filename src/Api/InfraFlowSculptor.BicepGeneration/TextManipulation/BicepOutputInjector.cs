using System.Text;
using System.Text.RegularExpressions;
using static InfraFlowSculptor.BicepGeneration.TextManipulation.BicepTextManipulationHelpers;

namespace InfraFlowSculptor.BicepGeneration.TextManipulation;

/// <summary>
/// Describes an output declaration to inject into a module: its name, its Bicep expression,
/// and whether it must be marked <c>@secure()</c>.
/// </summary>
public readonly record struct ModuleOutputInjection(
    string OutputName,
    string BicepExpression,
    bool IsSecure);

/// <summary>
/// Injects <c>output</c> declarations into a generated Bicep module for outputs referenced by
/// app settings on other resources. Sensitive outputs are decorated with <c>@secure()</c>.
/// </summary>
public static class BicepOutputInjector
{
    /// <summary>
    /// Appends <paramref name="outputs"/> to the module template. Skips outputs that already exist
    /// or whose root expression symbol is not declared in the module.
    /// </summary>
    public static string Inject(string moduleBicep, IReadOnlyCollection<ModuleOutputInjection> outputs)
    {
        var declaredSymbols = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in ResourceSymbolPattern.Matches(moduleBicep))
        {
            declaredSymbols.Add(match.Groups[1].Value);
        }

        var sb = new StringBuilder(moduleBicep.TrimEnd());

        foreach (var (outputName, bicepExpression, isSecure) in outputs)
        {
            if (HasOutput(moduleBicep, outputName))
                continue;

            var rootSymbol = ExtractRootSymbol(bicepExpression);
            if (rootSymbol is not null && !declaredSymbols.Contains(rootSymbol))
                continue;

            sb.AppendLine();
            if (isSecure)
            {
                sb.AppendLine();
                sb.AppendLine("@secure()");
                sb.Append($"output {outputName} string = {bicepExpression}");
            }
            else
            {
                sb.AppendLine();
                sb.Append($"output {outputName} string = {bicepExpression}");
            }
        }

        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>
    /// Extracts the root resource symbol from a Bicep expression. Returns <c>null</c> when the
    /// expression is a literal or cannot be parsed.
    /// </summary>
    /// <example>
    /// <c>"kv.properties.vaultUri"</c> returns <c>"kv"</c>; <c>"'${sqlServer.properties.fqdn}'"</c>
    /// returns <c>"sqlServer"</c>.
    /// </example>
    public static string? ExtractRootSymbol(string bicepExpression)
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
            {
                return rest[..dotPos].ToString();
            }
        }

        return null;
    }
}
