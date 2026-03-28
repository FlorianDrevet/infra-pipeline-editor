using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetPipelineFileContent;

/// <summary>Query to get the content of a specific generated pipeline file.</summary>
public record GetPipelineFileContentQuery(
    Guid InfrastructureConfigId,
    string FilePath
) : IRequest<ErrorOr<GetPipelineFileContentResult>>;

/// <summary>Result containing the file content.</summary>
public record GetPipelineFileContentResult(string Content);
