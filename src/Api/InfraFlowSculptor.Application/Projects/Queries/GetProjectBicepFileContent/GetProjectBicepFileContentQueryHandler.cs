using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectBicepFileContent;

public sealed class GetProjectBicepFileContentQueryHandler(
    IProjectAccessService accessService,
    IBlobService blobService)
    : IQueryHandler<GetProjectBicepFileContentQuery, GetProjectBicepFileContentResult>
{
    public async Task<ErrorOr<GetProjectBicepFileContentResult>> Handle(
        GetProjectBicepFileContentQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(
            new(query.ProjectId),
            cancellationToken);

        if (accessResult.IsError)
            return accessResult.Errors;

        var contentResult = await BlobDownloadHelper.GetLatestBlobContentAsync(
            blobService,
            blobPrefix: $"bicep/project/{query.ProjectId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.BicepFilesNotFoundError,
            entityId: query.ProjectId,
            fileNotFoundErrorFactory: Errors.Project.BicepFileNotFoundError,
            requestedFilePath: query.FilePath,
            candidateRelativePaths: [query.FilePath]);
        if (contentResult.IsError)
            return contentResult.Errors;

        return new GetProjectBicepFileContentResult(contentResult.Value);
    }
}