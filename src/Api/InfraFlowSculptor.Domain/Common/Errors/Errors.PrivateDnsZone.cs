using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Private DNS Zone aggregate.</summary>
    public static class PrivateDnsZone
    {
        /// <summary>Returned when a Private DNS Zone with the specified identifier does not exist.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "PrivateDnsZone.NotFound",
            description: $"A private DNS zone with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
