using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
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

        var prefix = $"pipeline/project/{query.ProjectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Error.NotFound(
                "ProjectPipelineFile.NotFound",
                $"No generated pipeline files found for project '{query.ProjectId}'.");

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(4)))
            .Distinct()
            .OrderDescending()
            .First();

        var blobName = $"{latestPrefix}/{query.FilePath}";
        var content = await blobService.DownloadContentAsync(blobName);

        if (content is null)
            return Error.NotFound(
                "ProjectPipelineFile.NotFound",
                $"File '{query.FilePath}' was not found.");

        return new GetProjectPipelineFileContentResult(content);
    }
}
