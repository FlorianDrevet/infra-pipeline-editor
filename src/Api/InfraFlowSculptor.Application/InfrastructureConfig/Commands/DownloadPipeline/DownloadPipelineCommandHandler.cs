using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadPipeline;

/// <summary>Handles the <see cref="DownloadPipelineCommand"/>.</summary>
public sealed class DownloadPipelineCommandHandler(
    IGeneratedArtifactService artifactService,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DownloadPipelineCommand, DownloadPipelineResult>
{
    public async Task<ErrorOr<DownloadPipelineResult>> Handle(
        DownloadPipelineCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfrastructureConfigId);

        var accessResult = await accessService.VerifyReadAccessAsync(configId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var result = await artifactService.DownloadLatestAsync(
            "pipeline", command.InfrastructureConfigId, cancellationToken);

        if (result is null)
            return Errors.InfrastructureConfig.PipelineFilesNotFoundError(command.InfrastructureConfigId);

        return new DownloadPipelineResult(result.Value.ZipContent, result.Value.FileName);
    }
}
