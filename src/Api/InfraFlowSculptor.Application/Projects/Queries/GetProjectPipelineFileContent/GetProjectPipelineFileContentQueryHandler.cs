using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectPipelineFileContent;

public sealed class GetProjectPipelineFileContentQueryHandler(
    IProjectAccessService accessService,
    IBlobService blobService)
    : IQueryHandler<GetProjectPipelineFileContentQuery, GetProjectPipelineFileContentResult>
{
    public async Task<ErrorOr<GetProjectPipelineFileContentResult>> Handle(
        GetProjectPipelineFileContentQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(
            new(query.ProjectId),
            cancellationToken);

        if (accessResult.IsError)
            return accessResult.Errors;

        var contentResult = await BlobDownloadHelper.GetLatestBlobContentAsync(
            blobService,
            blobPrefix: $"pipeline/project/{query.ProjectId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.PipelineFilesNotFoundError,
            entityId: query.ProjectId,
            options: new BlobDownloadHelper.LatestBlobContentOptions(
                Errors.Project.PipelineFileNotFoundError,
                query.FilePath,
                [
                    query.FilePath,
                    $"infra/{query.FilePath}",
                    $"app/{query.FilePath}",
                ]));
        if (contentResult.IsError)
            return contentResult.Errors;

        return new GetProjectPipelineFileContentResult(contentResult.Value);
    }
}
