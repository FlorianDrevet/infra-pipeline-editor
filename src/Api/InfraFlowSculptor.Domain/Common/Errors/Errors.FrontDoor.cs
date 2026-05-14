using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors related to the Front Door aggregate.</summary>
    public static class FrontDoor
    {
        /// <summary>Returned when a Front Door with the specified identifier does not exist.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "FrontDoor.NotFound",
            description: $"A front door with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );
    }
}
