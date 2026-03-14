using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    public static class RedisCache
    {
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "RedisCache.NotFound",
            description: $"A Redis cache with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
