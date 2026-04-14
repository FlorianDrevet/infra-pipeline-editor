using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Redis Cache aggregate.</summary>
    public static class RedisCache
    {
        /// <summary>Returned when a Redis cache with the specified identifier does not exist.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "RedisCache.NotFound",
            description: $"A Redis cache with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
