using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Services;
using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetPipelineFileContent;

/// <summary>Handles the <see cref="GetPipelineFileContentQuery"/>.</summary>
public sealed class GetPipelineFileContentQueryHandler(IGeneratedArtifactService artifactService)
    : IQueryHandler<GetPipelineFileContentQuery, GetPipelineFileContentResult>
{
    public async Task<ErrorOr<GetPipelineFileContentResult>> Handle(
        GetPipelineFileContentQuery query,
        CancellationToken cancellationToken)
    {
        var content = await artifactService.GetFileContentAsync(
            "pipeline", query.InfrastructureConfigId, query.FilePath, cancellationToken);

        if (content is null)
            return Error.NotFound(
                "PipelineFile.NotFound",
                $"File '{query.FilePath}' was not found.");

        return new GetPipelineFileContentResult(content);
    }
}
