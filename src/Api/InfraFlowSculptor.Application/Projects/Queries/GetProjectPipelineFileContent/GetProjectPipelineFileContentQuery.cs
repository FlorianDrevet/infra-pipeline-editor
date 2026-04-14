using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectPipelineFileContent;

public record GetProjectPipelineFileContentQuery(
    Guid ProjectId,
    string FilePath
) : IQuery<GetProjectPipelineFileContentResult>;

public record GetProjectPipelineFileContentResult(string Content);
