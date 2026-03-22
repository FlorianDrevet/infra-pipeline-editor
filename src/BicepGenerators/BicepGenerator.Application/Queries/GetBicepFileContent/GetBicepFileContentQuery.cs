using ErrorOr;
using MediatR;

namespace BicepGenerator.Application.Queries.GetBicepFileContent;

public record GetBicepFileContentQuery(
    Guid InfrastructureConfigId,
    string FilePath
) : IRequest<ErrorOr<GetBicepFileContentResult>>;

public record GetBicepFileContentResult(string Content);
