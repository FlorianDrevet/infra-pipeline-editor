using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBootstrap;

/// <summary>Command to download the latest generated bootstrap YAML files as a zip archive.</summary>
public record DownloadBootstrapCommand(
    Guid InfrastructureConfigId
) : ICommand<DownloadBootstrapResult>;

/// <summary>Result containing the zip archive bytes and file name.</summary>
public record DownloadBootstrapResult(
    byte[] ZipContent,
    string FileName);
