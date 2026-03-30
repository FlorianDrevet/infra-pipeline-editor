using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.GetBicepFileContent;

public record GetBicepFileContentQuery(
    Guid InfrastructureConfigId,
    string FilePath
) : IQuery<GetBicepFileContentResult>;

public record GetBicepFileContentResult(string Content);
