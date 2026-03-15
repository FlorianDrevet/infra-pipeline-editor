using ErrorOr;
using MediatR;

namespace BicepGenerator.Application.Commands.GenerateBicep;

public record GenerateBicepCommand(
    Guid InfrastructureConfigId
) : IRequest<ErrorOr<GenerateBicepResult>>;

public record GenerateBicepResult(
    Uri MainBicepUri,
    Uri ParametersUri,
    IReadOnlyDictionary<string, Uri> ModuleUris);
