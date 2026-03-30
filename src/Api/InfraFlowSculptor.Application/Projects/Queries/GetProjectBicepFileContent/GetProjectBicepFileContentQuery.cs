using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectBicepFileContent;

public record GetProjectBicepFileContentQuery(
    Guid ProjectId,
    string FilePath
) : IQuery<GetProjectBicepFileContentResult>;

public record GetProjectBicepFileContentResult(string Content);