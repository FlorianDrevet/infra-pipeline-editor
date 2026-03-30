using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadPipeline;

/// <summary>Handles the <see cref="DownloadPipelineCommand"/>.</summary>
public sealed class DownloadPipelineCommandHandler(IGeneratedArtifactService artifactService)
    : ICommandHandler<DownloadPipelineCommand, DownloadPipelineResult>
{
    public async Task<ErrorOr<DownloadPipelineResult>> Handle(
        DownloadPipelineCommand command,
        CancellationToken cancellationToken)
    {
        var result = await artifactService.DownloadLatestAsync(
            "pipeline", command.InfrastructureConfigId, cancellationToken);

        if (result is null)
            return Errors.InfrastructureConfig.PipelineFilesNotFoundError(command.InfrastructureConfigId);

        return new DownloadPipelineResult(result.Value.ZipContent, result.Value.FileName);
    }
}
