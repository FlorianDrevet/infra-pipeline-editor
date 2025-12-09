using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class InfrastructureConfig
    {
        public static Error NotFoundError(InfrastructureConfigId id) => Error.NotFound(
            code: "InfrastructureConfig.NotFound",
            description: $"A InfrastructureConfig with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}