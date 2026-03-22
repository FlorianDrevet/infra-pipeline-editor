using ErrorOr;
using MediatR;

namespace BicepGenerator.Application.Commands.DownloadBicep;

public record DownloadBicepCommand(
    Guid InfrastructureConfigId
) : IRequest<ErrorOr<DownloadBicepResult>>;

public record DownloadBicepResult(
    byte[] ZipContent,
    string FileName);
