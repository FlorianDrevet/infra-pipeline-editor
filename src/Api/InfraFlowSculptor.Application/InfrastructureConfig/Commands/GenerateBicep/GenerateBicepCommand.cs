using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

public record GenerateBicepCommand(
    Guid InfrastructureConfigId
) : IRequest<ErrorOr<GenerateBicepResult>>;

/// <summary>Result of Bicep generation, containing URIs to the generated artifact files.</summary>
/// <param name="MainBicepUri">URI to the main <c>main.bicep</c> file.</param>
/// <param name="ParameterFileUris">Map of parameter file names (e.g. <c>main.dev.bicepparam</c>) to their URIs.</param>
/// <param name="ModuleUris">Map of module paths to their URIs.</param>
public record GenerateBicepResult(
    Uri MainBicepUri,
    IReadOnlyDictionary<string, Uri> ParameterFileUris,
    IReadOnlyDictionary<string, Uri> ModuleUris);
