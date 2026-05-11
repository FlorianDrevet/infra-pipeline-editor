using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Redis Cache aggregate.</summary>
    public static class RedisCache
    {
        private const string InvalidMinimumTlsVersionCode = "RedisCache.InvalidMinimumTlsVersion";
        private const string InvalidSkuCode = "RedisCache.InvalidSku";
        private const string InvalidMaxMemoryPolicyCode = "RedisCache.InvalidMaxMemoryPolicy";

        /// <summary>Returned when a Redis cache with the specified identifier does not exist.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "RedisCache.NotFound",
            description: $"A Redis cache with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returns an error when a minimum TLS version string cannot be parsed into a valid enum value.</summary>
        public static Error InvalidMinimumTlsVersion(string raw) =>
            Error.Validation(
                code: InvalidMinimumTlsVersionCode,
                description: $"The minimum TLS version '{raw}' is not valid.");

        /// <summary>Returns an error when a Redis cache SKU string cannot be parsed into a valid enum value.</summary>
        public static Error InvalidSku(string raw) =>
            Error.Validation(code: InvalidSkuCode, description: $"The SKU '{raw}' is not valid.");

        /// <summary>Returns an error when a max memory policy string cannot be parsed into a valid enum value.</summary>
        public static Error InvalidMaxMemoryPolicy(string raw) =>
            Error.Validation(
                code: InvalidMaxMemoryPolicyCode,
                description: $"The max memory policy '{raw}' is not valid.");
    }
}
