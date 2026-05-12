using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBicepFileContent;

public sealed class GetBicepFileContentQueryHandler(IBlobService blobService)
    : IQueryHandler<GetBicepFileContentQuery, GetBicepFileContentResult>
{
    public async Task<ErrorOr<GetBicepFileContentResult>> Handle(
        GetBicepFileContentQuery query,
        CancellationToken cancellationToken)
    {
        var contentResult = await BlobDownloadHelper.GetLatestBlobContentAsync(
            blobService,
            blobPrefix: $"bicep/{query.InfrastructureConfigId}/",
            prefixSegmentCount: 3,
            notFoundErrorFactory: Errors.InfrastructureConfig.BicepFilesNotFoundError,
            entityId: query.InfrastructureConfigId,
            fileNotFoundErrorFactory: Errors.InfrastructureConfig.BicepFileNotFoundError,
            requestedFilePath: query.FilePath,
            candidateRelativePaths: [query.FilePath]);
        if (contentResult.IsError)
            return contentResult.Errors;

        return new GetBicepFileContentResult(contentResult.Value);
    }
}
