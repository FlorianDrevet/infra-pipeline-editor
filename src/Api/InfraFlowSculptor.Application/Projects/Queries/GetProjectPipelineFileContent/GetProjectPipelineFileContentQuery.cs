using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectPipelineFileContent;

public record GetProjectPipelineFileContentQuery(
    Guid ProjectId,
    string FilePath
) : IRequest<ErrorOr<GetProjectPipelineFileContentResult>>;

public record GetProjectPipelineFileContentResult(string Content);
