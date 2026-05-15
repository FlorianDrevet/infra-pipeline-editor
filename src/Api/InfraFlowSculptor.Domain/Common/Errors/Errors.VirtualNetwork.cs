using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Virtual Network aggregate.</summary>
    public static class VirtualNetwork
    {
        /// <summary>Returned when a Virtual Network with the specified identifier does not exist.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "VirtualNetwork.NotFound",
            description: $"A virtual network with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
