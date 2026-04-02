using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.InfrastructureConfig.Diagnostics;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

public record GenerateBicepCommand(
    Guid InfrastructureConfigId
) : ICommand<GenerateBicepResult>;

/// <summary>Result of Bicep generation, containing URIs to the generated artifact files.</summary>
/// <param name="MainBicepUri">URI to the main <c>main.bicep</c> file.</param>
/// <param name="ConstantsBicepUri">URI to the optional <c>constants.bicep</c> file (present only when role assignments are configured).</param>
/// <param name="ParameterFileUris">Map of parameter file names (e.g. <c>main.dev.bicepparam</c>) to their URIs.</param>
/// <param name="ModuleUris">Map of module paths to their URIs.</param>
/// <param name="Warnings">Diagnostic findings detected during generation.</param>
public record GenerateBicepResult(
    Uri MainBicepUri,
    Uri? ConstantsBicepUri,
    IReadOnlyDictionary<string, Uri> ParameterFileUris,
    IReadOnlyDictionary<string, Uri> ModuleUris,
    IReadOnlyList<ResourceDiagnosticItem> Warnings);
