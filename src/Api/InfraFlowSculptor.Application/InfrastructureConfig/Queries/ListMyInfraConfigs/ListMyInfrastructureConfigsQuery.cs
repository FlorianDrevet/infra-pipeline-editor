using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;

public record ListMyInfrastructureConfigsQuery : IRequest<ErrorOr<List<GetInfrastructureConfigResult>>>;
