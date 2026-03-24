using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="CosmosDbAggregate.CosmosDb"/> aggregate.</summary>
    public static class CosmosDb
    {
        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "CosmosDb.NotFound",
            description: $"A Cosmos DB account with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
