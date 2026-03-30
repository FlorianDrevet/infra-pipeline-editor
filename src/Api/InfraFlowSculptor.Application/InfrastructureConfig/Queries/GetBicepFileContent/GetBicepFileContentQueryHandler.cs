using InfraFlowSculptor.Application.Common.Interfaces;
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
        var prefix = $"bicep/{query.InfrastructureConfigId}/";
        var allBlobs = await blobService.ListBlobsAsync(prefix);

        if (allBlobs.Count == 0)
            return Errors.InfrastructureConfig.BicepFilesNotFoundError(query.InfrastructureConfigId);

        // Find the latest timestamp folder
        var latestPrefix = allBlobs
            .Select(b => string.Join('/', b.Split('/').Take(3)))
            .Distinct()
            .OrderDescending()
            .First();

        var blobName = $"{latestPrefix}/{query.FilePath}";
        var content = await blobService.DownloadContentAsync(blobName);

        if (content is null)
            return Errors.InfrastructureConfig.BicepFileNotFoundError(query.FilePath);

        return new GetBicepFileContentResult(content);
    }
}
