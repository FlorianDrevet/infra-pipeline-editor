using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBicep;

/// <summary>Handles the <see cref="DownloadProjectBicepCommand"/>.</summary>
public sealed class DownloadProjectBicepCommandHandler(
    IProjectAccessService accessService,
    IBlobService blobService)
    : ICommandHandler<DownloadProjectBicepCommand, DownloadProjectBicepResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<DownloadProjectBicepResult>> Handle(
        DownloadProjectBicepCommand command,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(command.ProjectId, cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var result = await BlobDownloadHelper.DownloadLatestBlobsAsZipAsync(
            blobService,
            blobPrefix: $"bicep/project/{command.ProjectId.Value}/",
            prefixSegmentCount: 4,
            fileNameSuffix: "bicep",
            notFoundErrorFactory: Errors.Project.BicepFilesNotFoundError,
            entityId: command.ProjectId.Value,
            cancellationToken);

        if (result.IsError)
            return result.Errors;

        return new DownloadProjectBicepResult(result.Value.Data, result.Value.FileName);
    }
}