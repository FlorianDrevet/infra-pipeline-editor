using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Application.Projects.Commands.DownloadProjectPipeline;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBootstrapPipeline;

/// <summary>Handles the <see cref="DownloadProjectBootstrapPipelineCommand"/>.</summary>
public sealed class DownloadProjectBootstrapPipelineCommandHandler(
    IProjectAccessService accessService,
    IBlobService blobService)
    : ICommandHandler<DownloadProjectBootstrapPipelineCommand, DownloadProjectPipelineResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<DownloadProjectPipelineResult>> Handle(
        DownloadProjectBootstrapPipelineCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(command.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var result = await BlobDownloadHelper.DownloadLatestBlobsAsZipAsync(
            blobService,
            blobPrefix: $"bootstrap/project/{command.ProjectId.Value}/",
            prefixSegmentCount: 4,
            fileNameSuffix: "bootstrap-pipeline",
            notFoundErrorFactory: Errors.Project.BootstrapFilesNotFoundError,
            entityId: command.ProjectId.Value,
            cancellationToken);

        if (result.IsError)
            return result.Errors;

        return new DownloadProjectPipelineResult(result.Value.Data, result.Value.FileName);
    }
}
