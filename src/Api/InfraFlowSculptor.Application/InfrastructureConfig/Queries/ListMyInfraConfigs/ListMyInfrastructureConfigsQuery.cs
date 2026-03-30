using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListMyInfraConfigs;

public record ListMyInfrastructureConfigsQuery : IQuery<List<GetInfrastructureConfigResult>>;
