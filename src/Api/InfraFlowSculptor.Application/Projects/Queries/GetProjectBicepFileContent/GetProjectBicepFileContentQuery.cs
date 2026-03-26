using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.Projects.Queries.GetProjectBicepFileContent;

public record GetProjectBicepFileContentQuery(
    Guid ProjectId,
    string FilePath
) : IRequest<ErrorOr<GetProjectBicepFileContentResult>>;

public record GetProjectBicepFileContentResult(string Content);