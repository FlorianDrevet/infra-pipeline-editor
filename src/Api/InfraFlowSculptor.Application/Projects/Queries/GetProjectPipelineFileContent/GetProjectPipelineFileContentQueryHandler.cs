using ErrorOr;
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

        var prefix = $"pipeline/project/{query.ProjectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.Project.PipelineFilesNotFoundError(query.ProjectId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(4)))
            .Distinct()
            .OrderDescending()
            .First();

        var candidateBlobNames = new[]
        {
            $"{latestPrefix}/{query.FilePath}",
            $"{latestPrefix}/infra/{query.FilePath}",
            $"{latestPrefix}/app/{query.FilePath}",
        };

        foreach (var blobName in candidateBlobNames)
        {
            var content = await blobService.DownloadContentAsync(blobName);
            if (content is not null)
                return new GetProjectPipelineFileContentResult(content);
        }

        return Errors.Project.PipelineFileNotFoundError(query.FilePath);
    }
}
