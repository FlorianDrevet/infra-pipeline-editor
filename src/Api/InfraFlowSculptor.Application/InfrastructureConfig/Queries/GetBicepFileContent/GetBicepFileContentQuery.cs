using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBicepFileContent;

public record GetBicepFileContentQuery(
    Guid InfrastructureConfigId,
    string FilePath
) : IRequest<ErrorOr<GetBicepFileContentResult>>;

public record GetBicepFileContentResult(string Content);
