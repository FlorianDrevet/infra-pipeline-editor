namespace InfraFlowSculptor.BicepGeneration.Assemblers;

/// <summary>
/// Result of <see cref="MainBicepAssembler.Generate"/>: the emitted <c>main.bicep</c> content
/// and the map of module outputs referenced during emission, keyed by module file path.
/// </summary>
/// <param name="Content">The full <c>main.bicep</c> text.</param>
/// <param name="UsedOutputsByModulePath">
/// Map of module file path → set of output names referenced from that module in <c>main.bicep</c>.
/// Keys are case-insensitive. Used by <see cref="Pipeline.Stages.IrOutputPruningStage"/> to prune
/// unused outputs from each module's spec without re-parsing the generated text.
/// </param>
internal sealed record MainBicepEmissionResult(
    string Content,
    IReadOnlyDictionary<string, IReadOnlyCollection<string>> UsedOutputsByModulePath);
