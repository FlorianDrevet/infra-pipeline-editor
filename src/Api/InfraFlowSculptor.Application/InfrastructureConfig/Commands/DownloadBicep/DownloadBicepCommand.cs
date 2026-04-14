using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBicep;

public record DownloadBicepCommand(
    Guid InfrastructureConfigId
) : ICommand<DownloadBicepResult>;

public record DownloadBicepResult(
    byte[] ZipContent,
    string FileName);
