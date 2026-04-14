using ErrorOr;
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

        var prefix = $"bicep/project/{query.ProjectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.Project.BicepFilesNotFoundError(query.ProjectId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(4)))
            .Distinct()
            .OrderDescending()
            .First();

        var blobName = $"{latestPrefix}/{query.FilePath}";
        var content = await blobService.DownloadContentAsync(blobName);

        if (content is null)
            return Errors.Project.BicepFileNotFoundError(query.FilePath);

        return new GetProjectBicepFileContentResult(content);
    }
}