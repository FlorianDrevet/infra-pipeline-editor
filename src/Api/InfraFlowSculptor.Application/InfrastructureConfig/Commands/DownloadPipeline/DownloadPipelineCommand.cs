using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadPipeline;

/// <summary>Command to download the latest generated pipeline YAML files as a zip archive.</summary>
public record DownloadPipelineCommand(
    Guid InfrastructureConfigId
) : ICommand<DownloadPipelineResult>;

/// <summary>Result containing the zip archive bytes and file name.</summary>
public record DownloadPipelineResult(
    byte[] ZipContent,
    string FileName);
