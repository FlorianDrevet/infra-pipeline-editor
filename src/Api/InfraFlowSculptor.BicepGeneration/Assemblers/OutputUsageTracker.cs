namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Tracks which module outputs are referenced while <see cref="MainBicepAssembler.Generate"/>
/// emits the contents of <c>main.bicep</c>. Replaces post-hoc regex parsing of the generated
/// text by recording every <c>{symbol}Module.outputs.{outputName}</c> emission and the path of
/// every <c>module {symbol} './{path}'</c> declaration as they are produced.
/// </summary>
/// <remarks>
/// Module symbols are matched case-insensitively. <see cref="Build"/> resolves usages whose
/// symbol was registered with a path; usages without a registered path (cross-config existing
/// resources, unmatched symbols) are silently ignored to mirror the legacy regex-based behaviour.
/// </remarks>
internal sealed class OutputUsageTracker
{
    private readonly Dictionary<string, string> _symbolToPath =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly List<(string Symbol, string OutputName)> _usages = [];

    /// <summary>
    /// Registers the module path emitted for <paramref name="moduleSymbol"/>. The first
    /// registration wins; subsequent registrations for the same symbol are ignored.
    /// </summary>
    /// <param name="moduleSymbol">The Bicep symbol of the module declaration (e.g. <c>kvModule</c>).</param>
    /// <param name="modulePath">The module file path relative to the configuration root
    /// (e.g. <c>modules/KeyVault/keyVault.module.bicep</c>).</param>
    public void RegisterModulePath(string moduleSymbol, string modulePath)
    {
        if (string.IsNullOrEmpty(moduleSymbol) || string.IsNullOrEmpty(modulePath))
        {
            return;
        }

        _symbolToPath.TryAdd(moduleSymbol, modulePath);
    }

    /// <summary>
    /// Registers a <c>{moduleSymbol}.outputs.{outputName}</c> reference emitted in
    /// <c>main.bicep</c>. Duplicates are tolerated and de-duplicated by <see cref="Build"/>.
    /// </summary>
    /// <param name="moduleSymbol">The Bicep symbol of the producing module.</param>
    /// <param name="outputName">The name of the output being consumed.</param>
    public void RegisterUsage(string moduleSymbol, string outputName)
    {
        if (string.IsNullOrEmpty(moduleSymbol) || string.IsNullOrEmpty(outputName))
        {
            return;
        }

        _usages.Add((moduleSymbol, outputName));
    }

    /// <summary>
    /// Builds the immutable map keyed by module file path, grouping the set of distinct
    /// output names referenced for each module. Path keys are case-insensitive.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Build()
    {
        var grouped = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (symbol, outputName) in _usages)
        {
            if (!_symbolToPath.TryGetValue(symbol, out var path))
            {
                continue;
            }

            if (!grouped.TryGetValue(path, out var outputs))
            {
                outputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                grouped[path] = outputs;
            }

            outputs.Add(outputName);
        }

        var result = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, outputs) in grouped)
        {
            result[path] = outputs;
        }

        return result;
    }
}
