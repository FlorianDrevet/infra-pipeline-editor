using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectPipeline;

/// <summary>Handles the <see cref="DownloadProjectPipelineCommand"/>.</summary>
public sealed class DownloadProjectPipelineCommandHandler(
    IProjectAccessService accessService,
    IBlobService blobService)
    : ICommandHandler<DownloadProjectPipelineCommand, DownloadProjectPipelineResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<DownloadProjectPipelineResult>> Handle(
        DownloadProjectPipelineCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(command.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var result = await BlobDownloadHelper.DownloadLatestBlobsAsZipAsync(
            blobService,
            blobPrefix: $"pipeline/project/{command.ProjectId.Value}/",
            prefixSegmentCount: 4,
            fileNameSuffix: "pipeline",
            notFoundErrorFactory: Errors.Project.PipelineFilesNotFoundError,
            entityId: command.ProjectId.Value,
            cancellationToken);

        if (result.IsError)
            return result.Errors;

        return new DownloadProjectPipelineResult(result.Value.Data, result.Value.FileName);
    }
}
