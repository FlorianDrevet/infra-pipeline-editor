using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadBicep;

public sealed class DownloadBicepCommandHandler(
    IBlobService blobService,
    IInfraConfigAccessService accessService)
    : ICommandHandler<DownloadBicepCommand, DownloadBicepResult>
{
    public async Task<ErrorOr<DownloadBicepResult>> Handle(
        DownloadBicepCommand command,
        CancellationToken cancellationToken)
    {
        // Audit APP-002 (2026-04-23): enforce read access before exposing artifacts.
        var accessResult = await accessService.VerifyReadAccessAsync(
            new InfrastructureConfigId(command.InfrastructureConfigId), cancellationToken);
        if (accessResult.IsError)
            return accessResult.Errors;

        var downloadResult = await BlobDownloadHelper.DownloadLatestBlobsAsZipAsync(
            blobService,
            blobPrefix: $"bicep/{command.InfrastructureConfigId}/",
            prefixSegmentCount: 3,
            fileNameSuffix: "bicep",
            notFoundErrorFactory: Errors.InfrastructureConfig.BicepFilesNotFoundError,
            entityId: command.InfrastructureConfigId,
            cancellationToken);
        if (downloadResult.IsError)
            return downloadResult.Errors;

        return new DownloadBicepResult(downloadResult.Value.Data, downloadResult.Value.FileName);
    }
}
