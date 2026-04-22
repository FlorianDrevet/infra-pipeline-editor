using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;
using MediatR;

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

        var prefix = $"bootstrap/project/{query.ProjectId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.Project.BootstrapFilesNotFoundError(query.ProjectId);

        var latestPrefix = allBlobs
            .Select(blobName => string.Join('/', blobName.Split('/').Take(4)))
            .Distinct()
            .OrderDescending()
            .First();

        var blobName = $"{latestPrefix}/{query.FilePath}";
        var content = await blobService.DownloadContentAsync(blobName);

        if (content is null)
            return Errors.Project.BootstrapFileNotFoundError(query.FilePath);

        return new GetProjectBootstrapPipelineFileContentResult(content);
    }
}
