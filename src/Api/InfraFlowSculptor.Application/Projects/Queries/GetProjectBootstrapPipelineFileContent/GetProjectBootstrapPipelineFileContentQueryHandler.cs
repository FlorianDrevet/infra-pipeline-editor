using ErrorOr;
using InfraFlowSculptor.Application.Common.Helpers;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectBootstrapPipelineFileContent;

/// <summary>Handles the <see cref="GetProjectBootstrapPipelineFileContentQuery"/>.</summary>
public sealed class GetProjectBootstrapPipelineFileContentQueryHandler(
    IProjectAccessService accessService,
    IBlobService blobService)
    : IQueryHandler<GetProjectBootstrapPipelineFileContentQuery, GetProjectBootstrapPipelineFileContentResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<GetProjectBootstrapPipelineFileContentResult>> Handle(
        GetProjectBootstrapPipelineFileContentQuery query,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.VerifyReadAccessAsync(
            new ProjectId(query.ProjectId),
            cancellationToken);

        if (accessResult.IsError)
            return accessResult.Errors;

        var contentResult = await BlobDownloadHelper.GetLatestBlobContentAsync(
            blobService,
            blobPrefix: $"bootstrap/project/{query.ProjectId}/",
            prefixSegmentCount: 4,
            notFoundErrorFactory: Errors.Project.BootstrapFilesNotFoundError,
            entityId: query.ProjectId,
            options: new BlobDownloadHelper.LatestBlobContentOptions(
                Errors.Project.BootstrapFileNotFoundError,
                query.FilePath,
                [query.FilePath]),
            cancellationToken);
        if (contentResult.IsError)
            return contentResult.Errors;

        return new GetProjectBootstrapPipelineFileContentResult(contentResult.Value);
    }
}
