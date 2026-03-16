using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class EnvironmentDefinition
    {
        public static Error NotFoundError(EnvironmentDefinitionId id) => Error.NotFound(
            code: "EnvironmentDefinition.NotFound",
            description: $"An environment definition with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
